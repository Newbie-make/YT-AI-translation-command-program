// These are the necessary libraries for the code to work.
// We added System.dll and Newtonsoft.Json.dll in the 'References' tab.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class CPHInline
{
    // --- ⚙️ START OF USER CONFIGURATION ---

    // This is a "secret code" between the bot and the AI. If the AI thinks a translation
    // would be a slur, it sends this code back instead of the translation.
    // Don't change this unless you also change it in the CreateSafeTranslationPrompt() method below.
    private const string HARMFUL_CONTENT_FLAG = "TRANSLATION_BLOCKED_HARMFUL";

    // These are your "paranoia" settings to stay within the Gemini API's free tier.
    // The bot will stop working for the day/minute if these limits are hit.
    // The official limits are higher, but these provide a safe buffer.
    private const int MAX_REQUESTS_PER_MINUTE = 15;
    private const int MAX_REQUESTS_PER_DAY = 450; 

    // --- END OF USER CONFIGURATION ---


    // This is the main function that runs when the command is used.
    public bool Execute()
    {
        // Gets information from Streamer.bot about the command user.
        string rawInput = args.ContainsKey("rawInput") ? args["rawInput"].ToString() : string.Empty;
        string user = args.ContainsKey("user") ? args["user"].ToString() : "Someone";

        // This is the first gatekeeper. It checks if you've hit your API usage limits.
        if (!IsRequestAllowed(user))
        {
            return false; // Stops everything if the rate limit is hit.
        }

        // Checks if the user just typed "!translate" with nothing after it.
        if (string.IsNullOrWhiteSpace(rawInput))
        {
            RefundRequestCount(); // Give back the "request credit" since we didn't use the API.
            CPH.SendYouTubeMessage($"Usage: !translate [lang_code] <text to translate>");
            return false;
        }
        
        // This is the input slur filter. It checks the user's message against your personal blocklist.
        try
        {
            // Gets your list of banned words from the "translateBlocklist" Global Variable in Streamer.bot.
            string blocklistStr = CPH.GetGlobalVar<string>("translateBlocklist", true);
            if (!string.IsNullOrEmpty(blocklistStr))
            {
                List<string> blockedWords = blocklistStr.Split(',').Select(s => s.Trim().ToLower()).ToList();
                string lowerInput = rawInput.ToLower();

                // If a bad word is found, stop and send a generic failure message.
                if (blockedWords.Any(word => !string.IsNullOrEmpty(word) && lowerInput.Contains(word)))
                {
                    RefundRequestCount(); // Refund the request since we didn't use the API.
                    CPH.SendYouTubeMessage($"Sorry @{user}, that message cannot be translated.");
                    CPH.LogError($"Blocked translation from {user} due to input filter match.");
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            CPH.LogError("Error during blocklist check: " + ex.Message);
        }

        // --- ⚙️ FLEXIBILITY: Add, Remove, or Change Languages Here ---
        // This is the dictionary of all supported languages.
        // The first part is the short code (e.g., "en"), the second is the full name sent to the AI.
        // To add a new language, just add a new line following the format: { "code", "Full Language Name" },
        var languageMap = new Dictionary<string, string>
        {
            // Real Languages
            { "en", "English" }, { "pt", "Portuguese" }, { "es", "Spanish" }, { "fr", "French" }, 
            { "de", "German" }, { "it", "Italian" }, { "ja", "Japanese" }, { "ko", "Korean" }, 
            { "ru", "Russian" }, 
            
            // Joke Languages
            { "sim", "Simlish" }, { "bin", "Binary code" }, { "morse", "Morse code" }, 
            { "baby", "incoherent baby babble" }, { "dk", "Donkey Kong style ape sounds" }
        };

        // This part of the code figures out if the user typed a language code or not.
        // It's best to leave this internal logic as is.
        string[] parts = rawInput.Split(new[] { ' ' }, 2);
        string targetLanguageCode = string.Empty;
        string textToTranslate = rawInput;

        if (parts.Length > 1 && languageMap.ContainsKey(parts[0].ToLower()))
        {
            targetLanguageCode = parts[0].ToLower();
            textToTranslate = parts[1];
        }

        // Gets your API key from the "geminiApiKey" Global Variable.
        string apiKey = CPH.GetGlobalVar<string>("geminiApiKey");
        // This is the URL for the AI model. If Google changes the model name, you would update it here.
        string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key={apiKey}";
        
        try
        {
            using (var client = new HttpClient())
            {
                string finalMessage;
                // SCENARIO 1: User specified a language (e.g., "!translate es hello").
                if (!string.IsNullOrEmpty(targetLanguageCode))
                {
                    string targetLanguageName = languageMap[targetLanguageCode];
                    string prompt = CreateSafeTranslationPrompt(textToTranslate, targetLanguageName);
                    finalMessage = PerformTranslation(client, url, prompt, user, $"to {targetLanguageName}");
                }
                // SCENARIO 2: No language specified, use the default bidirectional mode.
                else
                {
                    // First, ask the AI to detect the language of the input text.
                    string detectionPrompt = "What is the primary language of the following text? Respond with ONLY its two-letter ISO 639-1 code (e.g., 'en' for English, 'pt' for Portuguese). If you cannot determine the language, respond with 'unknown'.\n\nText: \"" + textToTranslate + "\"";
                    string detectedLangCode = PerformApiCall(client, url, detectionPrompt);

                    // --- ⚙️ FLEXIBILITY: Change Default Behavior Here ---
                    // This logic decides the default EN <-> Other Language translation.
                    // Currently, if the input is English ("en"), it translates to "Brazilian Portuguese".
                    // Otherwise, it translates to "English".
                    // You can change "Brazilian Portuguese" to "Spanish" or any other language you want as your default.
                    string targetLanguageName = (detectedLangCode == "en") ? "Brazilian Portuguese" : "English";
                    
                    string translationPrompt = CreateSafeTranslationPrompt(textToTranslate, targetLanguageName);
                    finalMessage = PerformTranslation(client, url, translationPrompt, user, $"to {targetLanguageName}");
                }
                // This is the line that sends the final message to YouTube chat.
                CPH.SendYouTubeMessage(finalMessage);
            }
        }
        catch (Exception ex)
        {
            CPH.LogError($"Translation Exception: {ex.Message}");
            CPH.SendYouTubeMessage($"Sorry @{user}, a bot error occurred.");
        }
        return true;
    }

    // --- HELPER METHODS (Internal logic, best not to change unless you know C#) ---

    // This method builds the detailed instructions we send to the AI.
    private string CreateSafeTranslationPrompt(string text, string targetLanguage)
    {
        string extraInstruction = "";
        // This adds a special rule for our joke languages to make them funnier.
        if (targetLanguage == "incoherent baby babble" || targetLanguage == "Donkey Kong style ape sounds")
        {
            extraInstruction = "IMPORTANT: The length of your response MUST be proportionate to the input text. A long input sentence requires a long response.";
        }
        return @$"You are a translation bot. Translate the following text into {targetLanguage}. {extraInstruction} Provide only the translation. Do not add any other text. --- TEXT TO TRANSLATE --- {text}";
    }

    // This is the low-level function that actually sends a request to the Gemini API and gets a response.
    private string PerformApiCall(HttpClient client, string url, string prompt)
    {
        var requestBody = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };
        var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
        var response = client.PostAsync(url, content).Result;
        if (!response.IsSuccessStatusCode) { throw new Exception("API call failed: " + response.Content.ReadAsStringAsync().Result); }
        string jsonResponse = response.Content.ReadAsStringAsync().Result;
        JObject parsedResponse = JObject.Parse(jsonResponse);
        return parsedResponse["candidates"][0]["content"]["parts"][0]["text"].ToString().Trim();
    }

    // This function takes the raw translation, checks it for our safety flag, and formats the final message for chat.
    private string PerformTranslation(HttpClient client, string url, string prompt, string user, string targetInfo)
    {
        try
        {
            string translatedText = PerformApiCall(client, url, prompt);
            // This checks for our safety flag from the AI.
            if (translatedText.Contains(HARMFUL_CONTENT_FLAG)) { CPH.LogError($"Blocked translation for {user} due to AI output filter."); return $"Sorry @{user}, that message cannot be translated."; }
            // This cleans up the text by removing any extra quotation marks the AI might add.
            if (translatedText.StartsWith("\"") && translatedText.EndsWith("\"")) { translatedText = translatedText.Substring(1, translatedText.Length - 2); }
            
            // --- ⚙️ FLEXIBILITY: Change the final message format here ---
            // This is the template for the message posted in chat. You can change how it looks.
            return $"Translation for {user} ({targetInfo}): \"{translatedText}\"";
        }
        catch (Exception ex) { CPH.LogError($"Gemini API Error (Translation): {ex.Message}"); return $"Sorry @{user}, I couldn't translate that right now."; }
    }

    // This is the engine for the rate limiter. It uses global variables to remember recent requests.
    private bool IsRequestAllowed(string user)
    {
        string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        string lastRequestDate = CPH.GetGlobalVar<string>("geminiLastRequestDate", true);
        int dailyCount = CPH.GetGlobalVar<int>("geminiRequestCountDaily", true);
        if (today != lastRequestDate) { dailyCount = 0; CPH.SetGlobalVar("geminiLastRequestDate", today, true); CPH.SetGlobalVar("geminiRequestCountDaily", 0, true); }
        if (dailyCount >= MAX_REQUESTS_PER_DAY) { CPH.LogError($"Daily rate limit hit. Request from {user} blocked."); CPH.SendYouTubeMessage($"@{user}, the translator has hit its daily usage limit. It will be back tomorrow!"); return false; }
        long sixtySecondsAgo = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 60;
        var timestampsStr = CPH.GetGlobalVar<string>("geminiRequestTimestamps", true);
        var recentTimestamps = new List<long>();
        if (!string.IsNullOrEmpty(timestampsStr)) { recentTimestamps = timestampsStr.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).Where(ts => ts > sixtySecondsAgo).ToList(); }
        if (recentTimestamps.Count >= MAX_REQUESTS_PER_MINUTE) { CPH.LogError($"Per-minute rate limit hit. Request from {user} blocked."); CPH.SendYouTubeMessage($"@{user}, the translator is on a short cooldown. Please try again in a moment!"); return false; }
        CPH.SetGlobalVar("geminiRequestCountDaily", dailyCount + 1, true);
        recentTimestamps.Add(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        CPH.SetGlobalVar("geminiRequestTimestamps", string.Join(",", recentTimestamps), true);
        return true;
    }

    // This helper just subtracts 1 from the daily counter if a request is blocked before using the API.
    private void RefundRequestCount()
    {
        CPH.SetGlobalVar("geminiRequestCountDaily", CPH.GetGlobalVar<int>("geminiRequestCountDaily", true) - 1, true);
    }
}