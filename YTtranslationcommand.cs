// These are the necessary libraries for the code to work.
// We added System.dll and Newtonsoft.Json.dll in the 'References' tab.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class CPHInline
{
    private class TextSegment
    {
        public string TextForApi { get; set; }
        public string Tone { get; set; } = "neutral";
        public List<string> ProperNouns { get; set; } = new List<string>();
        public Dictionary<string, string> PronounPlaceholders { get; set; } = new Dictionary<string, string>();
    }

    // --- ⚙️ START OF USER CONFIGURATION ---
    private const int MAX_REQUESTS_PER_MINUTE = 15;
    private const int MAX_REQUESTS_PER_DAY = 450;
    private const int YOUTUBE_CHAR_LIMIT = 200;
    // --- END OF USER CONFIGURATION ---

    public bool Execute()
    {
        string rawInput = args.ContainsKey("rawInput") ? args["rawInput"].ToString() : string.Empty;
        string user = args.ContainsKey("user") ? args["user"].ToString() : "Someone";

        if (!IsRequestAllowed(1, user, false)) return false;
        if (string.IsNullOrWhiteSpace(rawInput)) { SendYouTubeMessage("Usage: !translate [lang_code] [markers] <text>"); return false; }
        if (IsInputBlocked(rawInput, user)) return false;

        var languageMap = new Dictionary<string, string> {
            { "en", "English" }, { "pt", "Brazilian Portuguese" }, { "ptpt", "Portugal Portuguese" }, { "es", "Spanish" }, 
            { "fr", "French" }, { "de", "German" }, { "it", "Italian" }, { "ja", "Japanese" }, { "ko", "Korean" }, { "ru", "Russian" }, 
            { "sim", "Simlish" }, { "bin", "Binary code" }, { "morse", "Morse code" }, { "baby", "incoherent baby babble" }, { "dk", "Donkey Kong style ape sounds" }
        };

        var toneMap = new Dictionary<string, string> {
           { "joking", "joking" }, { "serious", "serious" }, { "sarcastic", "sarcastic" }, { "formal", "formal" }, { "casual", "casual" },
            { "brincando", "joking" }, { "sério", "serious" }, { "sarcástico", "sarcastic" },
            { "bromeando", "joking" }, { "serio", "serious" },
            { "plaisantant", "joking" }, { "sérieux", "serious" }, { "sarcastique", "sarcastic" },
            { "scherzend", "joking" }, { "ernst", "serious" }, { "sarkastisch", "sarcastic" },
            { "scherzoso", "joking" }, { "nongdam", "joking" }, { "jinji", "serious" }, { "shutlivyy", "joking" }
        };

        // --- ⚙️ THE MASTER PRONOUN MAP ---
        // This is where you define the final output for each language.
        // To add a new language, copy one of the blocks and customize it.
        var languagePronounMap = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            { 
                "Brazilian Portuguese", new Dictionary<string, string> {
                    { "she/her", "[ela]" },
                    { "he/him", "[ele]" },
                    { "they/them", "[delu]" }
                }
            },
            { 
                "Italian", new Dictionary<string, string> {
                    // For Italian, gender is shown in the adjective (amica/amico), so we don't need a marker.
                    // We simply remove the placeholder by replacing it with an empty string.
                    { "she/her", "[lei]" },
                    { "he/him", "[lui]" },
                    // For non-binary, you can decide on a convention. For now, we'll also remove it.
                    // You could change this to "[*]" or another marker if you prefer.
                    { "they/them", "" }
                }
            },
            {
                "Spanish", new Dictionary<string, string> {
                    { "she/her", "[ella]" }, // Same as Italian, a translated "amiga" is enough.
                    { "he/him", "[él]" },
                    { "they/them", "[elle]" } // Spanish uses "-e" endings, so a marker is optional.
                }
            }
        };

        string textForProcessing = rawInput;
        string targetLanguageCode = string.Empty;
        string[] initialParts = rawInput.Split(new[] { ' ' }, 2);
        if (initialParts.Length > 1 && languageMap.ContainsKey(initialParts[0].ToLower()))
        {
            targetLanguageCode = initialParts[0].ToLower();
            textForProcessing = initialParts[1];
        }

        var segments = new List<TextSegment>();
        // ... (The rest of the parsing logic is the same and correct)

        var toneParts = Regex.Split(textForProcessing, @"(&[\w/]+&)");
        string currentTone = "neutral";
        foreach (var part in toneParts) {
            if (string.IsNullOrWhiteSpace(part)) continue;
            if (part.StartsWith("&") && part.EndsWith("&")) {
                string userToneTag = part.Trim('&').ToLower();
                currentTone = toneMap.ContainsKey(userToneTag) ? toneMap[userToneTag] : userToneTag;
            } else {
                var segment = new TextSegment { Tone = currentTone };
                string processedText = part.Trim();
                var properNounRegex = new Regex(@"\*([^*]+)\*");
                foreach (Match match in properNounRegex.Matches(processedText)) { segment.ProperNouns.Add(match.Groups[1].Value); }
                processedText = properNounRegex.Replace(processedText, "$1");
                int pronounIndex = 0;
                var pronounRegex = new Regex(@"%([\w\s/-]+)%");
                processedText = pronounRegex.Replace(processedText, match => {
                    pronounIndex++;
                    string placeholder = $"[P{pronounIndex}]";
                    string pronoun = match.Groups[1].Value.ToLower();
                    segment.PronounPlaceholders[placeholder] = pronoun;
                    return placeholder;
                });
                segment.TextForApi = Regex.Replace(processedText, @"\s+", " ").Trim();
                if (string.IsNullOrWhiteSpace(segment.TextForApi)) continue;
                segments.Add(segment);
            }
        }
        
        if (!segments.Any()) return false;

        int requiredApiCallCount = string.IsNullOrEmpty(targetLanguageCode) ? segments.Count + 1 : segments.Count;
        if (!IsRequestAllowed(requiredApiCallCount, user, true)) return false;

        string apiKey = CPH.GetGlobalVar<string>("geminiApiKey");
        string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key={apiKey}";
        
        try
        {
            using (var client = new HttpClient())
            {
                var translatedSegments = new List<string>();
                string targetLanguageName;

                if (!string.IsNullOrEmpty(targetLanguageCode))
                {
                    targetLanguageName = languageMap[targetLanguageCode];
                }
                else
                {
                    string combinedTextForDetection = string.Join(" ", segments.Select(s => s.TextForApi));
                    string detectionPrompt = "What is the primary language of this text? Respond with ONLY its two-letter ISO 639-1 code...\n\nText: \"" + combinedTextForDetection + "\"";
                    string detectedLangCode = PerformApiCall(client, url, detectionPrompt);
                    targetLanguageName = (detectedLangCode == "en") ? "Brazilian Portuguese" : "English";
                }

                foreach (var segment in segments)
                {
                    string prompt = CreateTranslationPrompt(segment.TextForApi, targetLanguageName, segment.PronounPlaceholders, segment.ProperNouns, segment.Tone);
                    string translatedTextWithPlaceholders = PerformApiCall(client, url, prompt);

                    // --- C# RELIABLE REPLACEMENT LOGIC ---
                    string finalTranslatedText = translatedTextWithPlaceholders;
                    if (languagePronounMap.ContainsKey(targetLanguageName))
                    {
                        var replacementRules = languagePronounMap[targetLanguageName];
                        foreach(var entry in segment.PronounPlaceholders)
                        {
                            string placeholder = entry.Key; // e.g., "[P1]"
                            string pronoun = entry.Value;   // e.g., "she/her"
                            if(replacementRules.ContainsKey(pronoun))
                            {
                                finalTranslatedText = finalTranslatedText.Replace(placeholder, replacementRules[pronoun]);
                            }
                            else
                            {
                                finalTranslatedText = finalTranslatedText.Replace(placeholder, ""); // Default: remove if unknown
                            }
                        }
                    }
                    else
                    {
                        // Default behavior for languages not in the map: remove all placeholders
                        foreach(var entry in segment.PronounPlaceholders)
                        {
                            finalTranslatedText = finalTranslatedText.Replace(entry.Key, "");
                        }
                    }
                    translatedSegments.Add(Regex.Replace(finalTranslatedText, @"\s+", " ").Trim());
                }

                string finalMessage = $"Translation for {user} (to {targetLanguageName}): \"{string.Join(" ", translatedSegments)}\"";
                SendLongYouTubeMessage(finalMessage);
            }
        }
        catch (Exception ex)
        { CPH.LogError($"Translation Exception: {ex.Message}"); SendYouTubeMessage($"Sorry @{user}, a bot error occurred."); }
        return true;
    }

    private string CreateTranslationPrompt(string text, string targetLanguage, Dictionary<string, string> pronounPlaceholders, List<string> properNouns, string tone)
    {
        string genderInstruction = "";
        if (pronounPlaceholders.Any())
        {
            var instructions = pronounPlaceholders.Select(kvp => $"{kvp.Key} corresponds to '{kvp.Value}'");
            genderInstruction = @$"GENDER INSTRUCTIONS: The text contains placeholders like [P1], [P2]. You MUST translate the surrounding text to match the gender of the corresponding pronoun.
- Placeholder map: [{string.Join("; ", instructions)}].
- For 'they/them', use a modern, inclusive, gender-neutral singular form appropriate for the target language (e.g., using '-e' endings in Spanish/Portuguese). Do NOT use slash notation like 'o/a'.
- CRITICAL: You MUST KEEP the placeholders like [P1], [P2] in your final translated response exactly as they are.";
        }
        // ... (Rest of prompt is the same and correct)
        string properNounInstruction = "";
        if (properNouns.Any()) { properNounInstruction = $"PROPER NOUNS: The following words are proper nouns and MUST NOT be translated: [{string.Join(", ", properNouns)}]."; }
        string toneInstruction = "";
        if (!string.IsNullOrEmpty(tone) && tone != "neutral") { toneInstruction = $"TONE INSTRUCTION: The final translation MUST be delivered in a '{tone}' tone."; }
        string jokeInstruction = "";
        if (targetLanguage == "incoherent baby babble" || targetLanguage == "Donkey Kong style ape sounds") { jokeInstruction = "The length of your response MUST be proportionate to the input text."; }
        return @$"You are an expert and inclusive translation bot. Translate the following text into {targetLanguage}.
{genderInstruction}
{properNounInstruction}
{toneInstruction}
{jokeInstruction}
Provide only the translation. Do not add any conversational filler or quotation marks.
--- TEXT TO TRANSLATE ---
{text}";
    }
    
    // --- FULLY IMPLEMENTED HELPER METHODS ---
    private bool IsInputBlocked(string rawInput, string user) { try { string blocklistStr = CPH.GetGlobalVar<string>("translateBlocklist", true); if (!string.IsNullOrEmpty(blocklistStr) && blocklistStr.Split(',').Select(s => s.Trim().ToLower()).Any(word => !string.IsNullOrEmpty(word) && rawInput.ToLower().Contains(word))) { SendYouTubeMessage($"Sorry @{user}, that message cannot be translated."); CPH.LogError($"Blocked translation from {user} due to input filter match."); return true; } } catch (Exception ex) { CPH.LogError("Error during blocklist check: " + ex.Message); } return false; }
    private void SendLongYouTubeMessage(string message) { if (string.IsNullOrEmpty(message)) return; if (message.Length <= YOUTUBE_CHAR_LIMIT) { SendYouTubeMessage(message); return; } var chunks = new List<string>(); string remainingMessage = message; while (remainingMessage.Length > 0) { int maxChunkLength = YOUTUBE_CHAR_LIMIT - 7; if (remainingMessage.Length <= maxChunkLength) { chunks.Add(remainingMessage); break; } int splitIndex = remainingMessage.LastIndexOf(' ', maxChunkLength); if (splitIndex == -1) { splitIndex = maxChunkLength; } chunks.Add(remainingMessage.Substring(0, splitIndex)); remainingMessage = remainingMessage.Substring(splitIndex).Trim(); } for (int i = 0; i < chunks.Count; i++) { SendYouTubeMessage($"({i + 1}/{chunks.Count}) {chunks[i]}"); CPH.Wait(500); } }
    private void SendYouTubeMessage(string message) { CPH.SendYouTubeMessage(message); }
    private string PerformApiCall(HttpClient client, string url, string prompt) { var requestBody = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } }; var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json"); var response = client.PostAsync(url, content).Result; if (!response.IsSuccessStatusCode) { throw new Exception("API call failed: " + response.Content.ReadAsStringAsync().Result); } string jsonResponse = response.Content.ReadAsStringAsync().Result; JObject parsedResponse = JObject.Parse(jsonResponse); return parsedResponse["candidates"][0]["content"]["parts"][0]["text"].ToString().Trim().Trim('"'); }
    private bool IsRequestAllowed(int requestCount, string user, bool isFinalCheck) { string today = DateTime.UtcNow.ToString("yyyy-MM-dd"); string lastRequestDate = CPH.GetGlobalVar<string>("geminiLastRequestDate", true); int dailyCount = CPH.GetGlobalVar<int>("geminiRequestCountDaily", true); if (today != lastRequestDate) { dailyCount = 0; CPH.SetGlobalVar("geminiLastRequestDate", today, true); CPH.SetGlobalVar("geminiRequestCountDaily", 0, true); } if (dailyCount + requestCount > MAX_REQUESTS_PER_DAY) { SendYouTubeMessage($"@{user}, that command is too complex for the remaining daily API limit."); return false; } long sixtySecondsAgo = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 60; var timestampsStr = CPH.GetGlobalVar<string>("geminiRequestTimestamps", true); var recentTimestamps = new List<long>(); if (!string.IsNullOrEmpty(timestampsStr)) { recentTimestamps = timestampsStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).Where(ts => ts > sixtySecondsAgo).ToList(); } if (recentTimestamps.Count + requestCount > MAX_REQUESTS_PER_MINUTE) { SendYouTubeMessage($"@{user}, that command is too complex for the current rate limit. Please try again in a moment!"); return false; } if (isFinalCheck) { CPH.SetGlobalVar("geminiRequestCountDaily", dailyCount + requestCount, true); long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); for (int i = 0; i < requestCount; i++) { recentTimestamps.Add(now); } CPH.SetGlobalVar("geminiRequestTimestamps", string.Join(",", recentTimestamps), true); } return true; }
}
