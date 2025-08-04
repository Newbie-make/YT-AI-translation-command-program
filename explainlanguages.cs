using System;

public class CPHInline
{
    public bool Execute()
    {
        // --- ⚙️ USER CONFIGURATION ---
        // You can change the delay between messages (in milliseconds) and the text of each message here.
        
        int delayBetweenMessages = 750; // 0.75 seconds. A good value is between 500 and 1500.

        string message1 = "You can use !translate in two ways: 1. Default Mode (EN <-> PT): \"!translate hello\" or \"!translate olá\" | 2. Targeted Mode: \"!translate [code] <text>\".";
        
        string message2 = "Here are the codes: REAL: en (English), pt (Portuguese), es (Spanish), fr (French), de (German), it (Italian), ja (Japanese), ko (Korean), ru (Russian)";
        
        string message3 = "FUN: sim (Simlish), bin (Binary), morse (Morse Code), baby (Baby Babble), dk (Ape Sounds)";
        
        string message4 = "Mark gender by adding % around it (e.g., %she/her%), proper nouns with * (*Name*), and tone with & (&joking&, &sarcastic&, &serious&, &formal&, &casual&)";


        // --- Internal Logic (No need to edit) ---
        
        // Send the first message, then wait.
        CPH.SendYouTubeMessage(message1);
        CPH.Wait(delayBetweenMessages);

        // Send the second message, then wait.
        CPH.SendYouTubeMessage(message2);
        CPH.Wait(delayBetweenMessages);
        
        // Send the third message, then wait.
        CPH.SendYouTubeMessage(message3);
        CPH.Wait(delayBetweenMessages);

        // Send the final message.
        CPH.SendYouTubeMessage(message4);

        return true;
    }
}