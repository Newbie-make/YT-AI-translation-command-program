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
    // This is a small helper "blueprint" to organize the data for each part of a multi-tone message.
    private class TextSegment
    {
        public string TextForApi { get; set; }
        public string Tone { get; set; } = "neutral";
        public List<string> ProperNouns { get; set; } = new List<string>();
        public Dictionary<string, string> PronounPlaceholders { get; set; } = new Dictionary<string, string>();
    }

    // --- ⚙️ START OF USER CONFIGURATION ---
    // This section contains all the settings you can safely change.

    // These are your "paranoia" settings to stay within the Gemini API's free tier.
    // The bot will stop working for the day/minute if these limits are hit.
    private const int MAX_REQUESTS_PER_MINUTE = 15;
    private const int MAX_REQUESTS_PER_DAY = 450;
    
    // This is the character limit for a single YouTube chat message.
    private const int YOUTUBE_CHAR_LIMIT = 200;

    // This is the main function that runs when the command is used.
    public bool Execute()
    {
        // --- ⚙️ FLEXIBILITY: Add, Remove, or Change Languages Here ---
        // This is the dictionary of all supported languages.
        // The first part is the short code (e.g., "en"), the second is the full name sent to the AI.
        // To add a new language, just add a new line following the format: { "code", "Full Language Name" },
        var languageMap = new Dictionary<string, string> {
            { "en", "English" }, { "pt", "Brazilian Portuguese" }, { "ptpt", "Portugal Portuguese" }, { "es", "Spanish" }, 
            { "fr", "French" }, { "de", "German" }, { "it", "Italian" }, { "ja", "Japanese" }, { "ko", "Korean" }, { "ru", "Russian" }, 
            { "sim", "Simlish" }, { "bin", "Binary code" }, { "morse", "Morse code" }, 
            { "baby", "incoherent baby babble" }, { "dk", "Donkey Kong style ape sounds" }
        };

        // --- ⚙️ FLEXIBILITY: Add, Remove, or Change Localized Tone Tags Here ---
        // This maps user-typed tone tags (in various languages) to standardized English instructions for the AI.
        var toneMap = new Dictionary<string, string> {
           { "joking", "joking" }, { "serious", "serious" }, { "sarcastic", "sarcastic" }, { "formal", "formal" }, { "casual", "casual" },
            { "brincando", "joking" }, { "sério", "serious" }, { "sarcástico", "sarcastic" },
            { "bromeando", "joking" }, { "serio", "serious" },
            { "plaisantant", "joking" }, { "sérieux", "serious" }, { "sarcastique", "sarcastic" },
            { "scherzend", "joking" }, { "ernst", "serious" }, { "sarkastisch", "sarcastic" },
            { "scherzoso", "joking" },
            { "nongdam", "joking" }, { "jinji", "serious" },
            { "shutlivyy", "joking" }
        };

        // --- ⚙️ THE MASTER PRONOUN MAP ---
        // This is the most powerful customization. It defines the final text that replaces the AI's placeholders
        // for each language, giving you full control over the output.
        // To add rules for a new language, copy a block, change the language name (must match languageMap), and define the outputs.
        var languagePronounMap = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            { 
                "Brazilian Portuguese", new Dictionary<string, string> {
                    { "she/her", "[ela]" },
                    { "he/him", "[ele]" },
                    { "they/them", "[delu]" }
                    // Add other pronouns like "xe/xem" here if you wish.
                }
            },
            { 
                "Italian", new Dictionary<string, string> {
                    // For Italian, gender is often shown in the adjective (e.g., amica/amico).
                    // A marker can feel redundant. You can choose to add one or remove it.
                    { "she/her", "[lei]" },  // We will add the pronoun for clarity.
                    { "he/him", "[lui]" },   // We will add the pronoun for clarity.
                    // For non-binary, if there isn't a single perfect word, you can use a marker or remove it.
                    // Using "" (empty string) removes the placeholder entirely.
                    { "they/them", "[loro]" } 
                }
            },
            {
                "Spanish", new Dictionary<string, string> {
                    { "she/her", "[ella]" },
                    { "he/him", "[él]" },
                    { "they/them", "[elle]" } // "elle" is a common neopronoun in Spanish.
                }
            }
        };

        // --- END OF USER CONFIGURATION ---


        // --- START OF INTERNAL LOGIC (Best not to edit below here unless you know C#) ---

        // Gets information from Streamer.bot about the command user.
        string rawInput = args.ContainsKey("rawInput") ? args["rawInput"].ToString() : string.Empty;
        string user = args.ContainsKey("user") ? args["user"].ToString() : "Someone";

        // Initial checks for API limits, empty input, and blocked words.
        if (!IsRequestAllowed(1, user, false)) return false;
        if (string.IsNullOrWhiteSpace(rawInput)) { SendYouTubeMessage("Usage: !translate [lang_code] [markers] <text>"); return false; }
        if (IsInputBlocked(rawInput, user)) return false;

        // Step 1: Check for and extract the language code FIRST from the raw input.
        string textForProcessing = rawInput;
        string targetLanguageCode = string.Empty;
        string[] initialParts = rawInput.Split(new[] { ' ' }, 2);
        if (initialParts.Length > 1 && languageMap.ContainsKey(initialParts[0].ToLower()))
        {
            targetLanguageCode = initialParts[0].ToLower();
            textForProcessing = initialParts[1]; // The rest of the string is now the text to translate.
        }

        // Step 2: Begin parsing the text. We break it into segments based on tone markers.
        var segments = new List<TextSegment>();
        var toneParts = Regex.Split(textForProcessing, @"(&[\w/]+&)");
        string currentTone = "neutral";

        foreach (var part in toneParts)
        {
            if (string.IsNullOrWhiteSpace(part)) continue;
            
            // If the part is a tone marker (e.g., &joking&), set it as the current tone for the next text segment.
            if (part.StartsWith("&") && part.EndsWith("&"))
            {
                string userToneTag = part.Trim('&').ToLower();
                currentTone = toneMap.ContainsKey(userToneTag) ? toneMap[userToneTag] : userToneTag;
            }
            // Otherwise, it's text. We process it.
            else
            {
                var segment = new TextSegment { Tone = currentTone };
                string processedText = part.Trim();

                // Process *Proper Nouns* first. We tell the AI not to translate them.
                var properNounRegex = new Regex(@"\*([^*]+)\*");
                foreach (Match match in properNounRegex.Matches(processedText)) { segment.ProperNouns.Add(match.Groups[1].Value); }
                processedText = properNounRegex.Replace(processedText, "$1");

                // Process %Pronouns% using the Placeholder Strategy.
                // This replaces the pronoun with a unique tag like [P1] and remembers the mapping.
                // This preserves the context for the AI.
                int pronounIndex = 0;
                var pronounRegex = new Regex(@"%([\w\s/-]+)%");
                processedText = pronounRegex.Replace(processedText, match => {
                    pronounIndex++;
                    string placeholder = $"[P{pronounIndex}]";
                    string pronoun = match.Groups[1].Value.ToLower(); // Standardize to lowercase
                    segment.PronounPlaceholders[placeholder] = pronoun;
                    return placeholder;
                });

                segment.TextForApi = Regex.Replace(processedText, @"\s+", " ").Trim();
                if (string.IsNullOrWhiteSpace(segment.TextForApi)) continue;
                segments.Add(segment);
            }
        }
        
        // If after all that, there's nothing to translate, stop.
        if (!segments.Any()) return false;

        // Final check: Do we have enough API calls for the work we need to do?
        int requiredApiCallCount = string.IsNullOrEmpty(targetLanguageCode) ? segments.Count + 1 : segments.Count;
        if (!IsRequestAllowed(requiredApiCallCount, user, true)) return false;

        // Get the API key from Global Variables and set up the URL for the AI model.
        string apiKey = CPH.GetGlobalVar<string>("geminiApiKey");
        string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key={apiKey}";
        
        try
        {
            using (var client = new HttpClient())
            {
                var translatedSegments = new List<string>();
                string targetLanguageName;

                // Determine the final target language (either user-specified or auto-detected).
                if (!string.IsNullOrEmpty(targetLanguageCode))
                {
                    targetLanguageName = languageMap[targetLanguageCode];
                }
                else
                {
                    // If no language code was given, ask the AI to detect it.
                    string combinedTextForDetection = string.Join(" ", segments.Select(s => s.TextForApi));
                    string detectionPrompt = "What is the primary language of this text? Respond with ONLY its two-letter ISO 639-1 code...\n\nText: \"" + combinedTextForDetection + "\"";
                    string detectedLangCode = PerformApiCall(client, url, detectionPrompt);
                    // Default behavior: if English, translate to Portuguese; otherwise, translate to English.
                    targetLanguageName = (detectedLangCode == "en") ? "Brazilian Portuguese" : "English";
                }

                // Loop through each segment and translate it with its own specific tone and markers.
                foreach (var segment in segments)
                {
                    string prompt = CreateTranslationPrompt(segment.TextForApi, targetLanguageName, segment.PronounPlaceholders, segment.ProperNouns, segment.Tone);
                    string translatedTextWithPlaceholders = PerformApiCall(client, url, prompt);

                    // --- C# RELIABLE REPLACEMENT LOGIC ---
                    // The AI returns the text with placeholders. Now, our code does the final, perfect replacement.
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
                                finalTranslatedText = finalTranslatedText.Replace(placeholder, ""); // Default: remove if pronoun is unknown
                            }
                        }
                    }
                    else
                    {
                        // Default behavior for languages not in our map: just remove all placeholders.
                        foreach(var entry in segment.PronounPlaceholders)
                        {
                            finalTranslatedText = finalTranslatedText.Replace(entry.Key, "");
                        }
                    }
                    // Clean up any extra spaces that might have been created during replacement.
                    translatedSegments.Add(Regex.Replace(finalTranslatedText, @"\s+", " ").Trim());
                }

                // Stitch all the translated segments back together into one final message.
                string finalMessage = $"Translation for {user} (to {targetLanguageName}): \"{string.Join(" ", translatedSegments)}\"";
                SendLongYouTubeMessage(finalMessage);
            }
        }
        catch (Exception ex)
        { CPH.LogError($"Translation Exception: {ex.Message}"); SendYouTubeMessage($"Sorry @{user}, a bot error occurred."); }
        return true;
    }

    // --- HELPER METHODS (Internal logic, best not to change unless you know C#) ---

    // This method builds the detailed instructions for the AI.
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
    
    // This checks the user's message against a blocklist defined in a Streamer.bot global variable.
    private bool IsInputBlocked(string rawInput, string user) { try { string blocklistStr = CPH.GetGlobalVar<string>("translateBlocklist", true); if (!string.IsNullOrEmpty(blocklistStr) && blocklistStr.Split(',').Select(s => s.Trim().ToLower()).Any(word => !string.IsNullOrEmpty(word) && rawInput.ToLower().Contains(word))) { SendYouTubeMessage($"Sorry @{user}, that message cannot be translated."); CPH.LogError($"Blocked translation from {user} due to input filter match."); return true; } } catch (Exception ex) { CPH.LogError("Error during blocklist check: " + ex.Message); } return false; }
    
    // This handles sending long messages by breaking them into smaller chunks that fit YouTube's character limit.
    private void SendLongYouTubeMessage(string message) { if (string.IsNullOrEmpty(message)) return; if (message.Length <= YOUTUBE_CHAR_LIMIT) { SendYouTubeMessage(message); return; } var chunks = new List<string>(); string remainingMessage = message; while (remainingMessage.Length > 0) { int maxChunkLength = YOUTUBE_CHAR_LIMIT - 7; if (remainingMessage.Length <= maxChunkLength) { chunks.Add(remainingMessage); break; } int splitIndex = remainingMessage.LastIndexOf(' ', maxChunkLength); if (splitIndex == -1) { splitIndex = maxChunkLength; } chunks.Add(remainingMessage.Substring(0, splitIndex)); remainingMessage = remainingMessage.Substring(splitIndex).Trim(); } for (int i = 0; i < chunks.Count; i++) { SendYouTubeMessage($"({i + 1}/{chunks.Count}) {chunks[i]}"); CPH.Wait(500); } }
    
    // A shortcut for sending a YouTube message.
    private void SendYouTubeMessage(string message) { CPH.SendYouTubeMessage(message); }
    
    // This performs the actual web request to the Google Gemini API.
    private string PerformApiCall(HttpClient client, string url, string prompt) { var requestBody = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } }; var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json"); var response = client.PostAsync(url, content).Result; if (!response.IsSuccessStatusCode) { throw new Exception("API call failed: " + response.Content.ReadAsStringAsync().Result); } string jsonResponse = response.Content.ReadAsStringAsync().Result; JObject parsedResponse = JObject.Parse(jsonResponse); return parsedResponse["candidates"][0]["content"]["parts"][0]["text"].ToString().Trim().Trim('"'); }
    
    // This manages the daily and per-minute API rate limits to prevent overuse.
    private bool IsRequestAllowed(int requestCount, string user, bool isFinalCheck) { string today = DateTime.UtcNow.ToString("yyyy-MM-dd"); string lastRequestDate = CPH.GetGlobalVar<string>("geminiLastRequestDate", true); int dailyCount = CPH.GetGlobalVar<int>("geminiRequestCountDaily", true); if (today != lastRequestDate) { dailyCount = 0; CPH.SetGlobalVar("geminiLastRequestDate", today, true); CPH.SetGlobalVar("geminiRequestCountDaily", 0, true); } if (dailyCount + requestCount > MAX_REQUESTS_PER_DAY) { SendYouTubeMessage($"@{user}, that command is too complex for the remaining daily API limit."); return false; } long sixtySecondsAgo = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 60; var timestampsStr = CPH.GetGlobalVar<string>("geminiRequestTimestamps", true); var recentTimestamps = new List<long>(); if (!string.IsNullOrEmpty(timestampsStr)) { recentTimestamps = timestampsStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).Where(ts => ts > sixtySecondsAgo).ToList(); } if (recentTimestamps.Count + requestCount > MAX_REQUESTS_per_MINUTE) { SendYouTubeMessage($"@{user}, that command is too complex for the current rate limit. Please try again in a moment!"); return false; } if (isFinalCheck) { CPH.SetGlobalVar("geminiRequestCountDaily", dailyCount + requestCount, true); long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); for (int i = 0; i < requestCount; i++) { recentTimestamps.Add(now); } CPH.SetGlobalVar("geminiRequestTimestamps", string.Join(",", recentTimestamps), true); } return true; }
}