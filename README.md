# YT-AI-translation-command-program
Bot Features:

    Translates messages from any language to English.

    Translates messages from English to Brazilian Portuguese (default).

    Can translate to other languages on command (e.g., !translate es hello).

    Includes fun "joke" languages like Simlish, Morse Code, and more.

    Has built-in safety filters to block slurs and harmful content.

    Includes a rate limiter to prevent you from ever being charged for API usage. 
    
Guide to my YouTube AI Translator Bot

    Download streamer.bot https://streamer.bot/

    In Streamer.bot, go to Platforms -> YouTube -> Accounts.

    Sign in with the Google Account that is the Owner or Moderator of your YouTube channel.

    When Google asks for permissions, CHECK ALL THE BOXES. You must grant it permission to "manage your YouTube account" and send messages.

Step 1: Get Your Google Cloud/Gemini API Key

    Go to Google AI Studio: aistudio.google.com and sign in.

    Click "Get API key" on the left, then "Create API key in new project".

    Copy your new API key and paste it into Notepad for now.

    Now, we must enable the API in the cloud console. Go to: console.cloud.google.com.

    Make sure the project selected at the top matches the one you just created.

    In the search bar, type Generative Language API and click on it.

    Click the ENABLE button.

    The Credit Card Step: If it prompts you to enable billing by adding a credit card, you will need to do so. Don't panic! This is primarily for identity verification and for if you go way over the free limits. The bot we built has a safety limiter so you will not be charged.

Step 2: Set Up Global Variables in Streamer.bot

(This is the bot's "memory")

    In Streamer.bot, go to Variables and right click to add a new global variable.

    Add the API Key:

        Right-click, Add.

        Name: geminiApiKey (case-sensitive!)

        Value: Paste your API key here.

        Click OK.

    Add the Slur Filter Blocklist:

        Right-click, Add.

        Name: translateBlocklist

        Value: Enter a comma-separated list of words to block (e.g., word1,word2,word3). You can find public lists on GitHub by searching "streamer blocklist".

        Check the Persist box.

        Click OK.

Step 3: Create the Main Translator Action

    In Streamer.bot, go to the Actions tab.

    Right-click, Add. Name it Translate Chat (Gemini YT). Click OK.

    In the Sub-Actions pane on the right, right-click and choose C# -> Execute C# Code.

    DELETE all the default code in the pop-up window.

    PASTE IN the complete, commented C# code you have. (You may alter it however you want. There are comments that let you know what is sae to 

    Add References (CRITICAL STEP):

        At the bottom of the C# window, click the References tab.

        Right-click, Add Reference from File... -> select Newtonsoft.Json.dll.

        Right-click again, Add Reference from File... -> scroll down and select System.dll.

        You should now have two files in your references list.

    Click the Compile button at the bottom. It MUST say "Compiled successfully!".

    Click OK.

Step 4: Create the YouTube Commands

    Go to Commands.

    Right-click, Add.

    Command: !translate. (You can start a new line and add as many or as little as you want)

    Action: Choose your Translate Chat (Gemini YT) action from the dropdown.

    Location: Make sure this is set to Start of message.

    Check the YouTube Message box under sources and unclick Twitch Message checkbox.

    Click OK.

    Repeat these steps to add any other command names you want (e.g., !traduzir, !honyaku).

Step 5: Create the !languages Helper Command

(Optional but highly recommended)

    Create the Action:

        Go to the Actions tab, Add a new action. Name it List Supported Languages YT.

        In Sub-Actions, add YouTube -> Send Message to Live Chat.

        Paste the pre-formatted list of languages into the Message box.

        Click OK.

    Create the Command:

        Go back to Platforms -> YouTube -> Commands.

        Add a new command named !languages.

        Set its Action to your new List Supported Languages YT action.

        Click OK.

Step 6: Test It!

Note: As this is an AI there are bound to be mistakes and odd things of the sort at times.
Go live on YouTube (an "Unlisted" stream is perfect for testing) and try out your new commands!

Note: As this is an AI there are bound to be mistakes and odd things of the sort at times.
