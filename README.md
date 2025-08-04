# YT-AI-translation-command-program
Bot Features:

    Translates messages from other languages into English by default.

    Translates messages from English into Brazilian Portuguese by default.

    Can translate to other languages on command (e.g., !translate es hello).

    Includes fun "joke" languages like Simlish, Morse Code, and more.

    Has a manual input filter for words you want to block.

    Includes a rate limiter to prevent you from ever being charged for API usage. 

    Smartly handles gendered languages using optional %she/her% and *ProperNoun* markers.

    Can handle multiple tones in a single sentence (e.g., &sarcastic& ... &serious& ...).

    Automatically splits long messages that would otherwise fail to send.
    
Guide to my YouTube AI Translator Bot

    Download YTtranslationcommand.cs on this GitHub page
    
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

    Add the Slur Filter Blocklist (If you don't want to block any words then skip this step entirely):

        Right-click, Add.

        Name: translateBlocklist

        Value: Enter a comma-separated list of words to block (e.g., word1,word2,word3). You can find public lists on GitHub by searching "streamer blocklist". 
        Click OK.

    Add the Daily Counter:

        Right-click, Add.

        Name: geminiRequestCountDaily

        Value: 0

        Click OK.

    Add the Rate Limit Tracker:

        Right-click, Add.

        Name: geminiRequestTimestamps

        Value: 0

        Click OK.

Step 3: Create the Main Translator Action

    In Streamer.bot, go to the Actions tab.

    Right-click, Add. Name it Translate Chat (Gemini YT). Click OK.

    In the Sub-Actions pane on the right, right-click and choose C# -> Execute C# Code.

    DELETE all the default code in the pop-up window.

    PASTE IN the complete, commented C# code you have. (You may alter it however you want. There are comments that let you know what is safe to change)

    Add References:

        At the bottom of the C# window, click the References tab.

        Right-click, Add Reference from File... -> select Newtonsoft.Json.dll.

        Right-click again, Add Reference from File... -> scroll down and select System.dll.

        Right-click again, Add Reference from File... -> System.Text.RegularExpressions.dll.

        You should now have two files in your references list.

    Click the Compile button at the bottom. It MUST say "Compiled successfully!".

    Click OK.

Step 4: Create the YouTube Commands

    Go to Commands.

    Right-click, Add.

    Command: !translate. (You can start a new line and add as many or as little as you want)

    Location: Make sure this is set to Start of message.

    Check the YouTube Message box under sources and unclick Twitch Message checkbox.

    Click OK.

    Repeat these steps to add any other command names you want (e.g., !traduzir, !honyaku).

Step 5: Create the !languages Helper Command

(Optional but highly recommended)

    Create the Command:

        Go back to Commands.

        Add a new command named !languages.

        Command: !translate. (You can start a new line and add as many or as little as you want)

        Location: Make sure this is set to Start of message.

        Check the YouTube Message box under sources and unclick Twitch Message checkbox.

        Click OK.

    
    Create the Action:

        Go to the Actions tab, Add a new action. Name it Explain Languages or something like that.

        In Sub-Actions, add Core -> C# -> Excute C# code.

        Delete all code in the pop up window.

        Paste the explainlanguages.cs code.

        Click OK.

        In Triggers, Core -> Commands -> Commands triggered.

        In the dropdown choose the command you just created.

        Click OK.

    
Step 6: Test It!

Note: As this is an AI there are bound to be mistakes and odd things of the sort at times.


-----------------------------

# Programa de Comando de Tradução com IA para o YouTube

Recursos do Bot:

    Traduz mensagens de outros idiomas para o inglês por padrão.

    Traduz mensagens do inglês para o Português do Brasil por padrão.

    Pode traduzir para outros idiomas com um comando (ex: !translate es hello).

    Inclui idiomas divertidos de "brincadeira", como Simlish, Código Morse e mais.

    Possui um filtro de entrada manual para palavras que você deseja bloquear.

    Inclui um limitador de uso para evitar que você seja cobrado pelo uso da API.

    Lida inteligentemente com idiomas que usam gênero, usando marcadores opcionais %she/her% e *ProperNoun*.

    Pode lidar com múltiplos tons em uma única frase (ex: &sarcástico& ... &sério& ...).

    Divide automaticamente mensagens longas que, de outra forma, não seriam enviadas.

Guia para o meu Bot Tradutor de IA do YouTube

    Baixe o arquivo YTtranslationcommand.cs nesta página do GitHub.

    Baixe o Streamer.bot: https://streamer.bot/

    No Streamer.bot, vá para Platforms -> YouTube -> Accounts.

    Faça login com a Conta Google que é Dona (Owner) ou Moderadora (Moderator) do seu canal do YouTube.

    Quando o Google pedir permissões, MARQUE TODAS AS CAIXAS. Você precisa conceder permissão para "gerenciar sua conta do YouTube" e enviar mensagens.

Passo 1: Obtenha sua Chave de API do Google Cloud/Gemini

    Acesse o Google AI Studio: aistudio.google.com e faça login.

    Clique em "Get API key" à esquerda, e depois em "Create API key in new project".

    Copie sua nova chave de API e cole-a no Bloco de Notas (Notepad) por enquanto.

    Agora, precisamos ativar a API no console da nuvem. Acesse: console.cloud.google.com.

    Verifique se o projeto selecionado no topo da página é o mesmo que você acabou de criar.

    Na barra de pesquisa, digite Generative Language API e clique nela.

    Clique no botão ENABLE (ATIVAR).

    O Passo do Cartão de Crédito: Se for solicitado que você ative o faturamento adicionando um cartão de crédito, você precisará fazer isso. Fique tranquilo! Isso é principalmente para verificação de identidade e para casos em que você ultrapasse muito os limites gratuitos. O bot que criamos tem um limitador de segurança para que você não seja cobrado.

Passo 2: Configure as Variáveis Globais no Streamer.bot

(Esta é a "memória" do bot)

    No Streamer.bot, vá para a aba Variables e clique com o botão direito para adicionar uma nova variável global.

    Adicione a Chave de API:

        Clique com o botão direito, Add.

        Name: geminiApiKey (sensível a maiúsculas e minúsculas!)

        Value: Cole sua chave de API aqui.

        Clique em OK.

    Adicione a Blocklist (Lista de Bloqueio) do Filtro de Ofensas (Se você não quiser bloquear nenhuma palavra, pule esta etapa inteiramente):

        Clique com o botão direito, Add.

        Name: translateBlocklist

        Value: Insira uma lista de palavras separadas por vírgula para bloquear (ex: palavra1,palavra2,palavra3). Você pode encontrar listas públicas no GitHub pesquisando por "streamer blocklist".

        Clique em OK.

    Adicione o Contador Diário:

        Clique com o botão direito, Add.

        Name: geminiRequestCountDaily

        Value: 0

        Clique em OK.

    Adicione o Rastreador de Limite de Uso:

        Clique com o botão direito, Add.

        Name: geminiRequestTimestamps

        Value: 0

        Clique em OK.

Passo 3: Crie a Ação Principal do Tradutor

    No Streamer.bot, vá para a aba Actions.

    Clique com o botão direito, Add. Nomeie como Translate Chat (Gemini YT). Clique em OK.

    No painel de Sub-Actions à direita, clique com o botão direito e escolha C# -> Execute C# Code.

    APAGUE todo o código padrão na janela que aparecer.

    COLE o código C# completo e comentado que você tem. (Você pode alterá-lo como quiser. Existem comentários que informam o que é seguro alterar).

    Adicione as Referências:

        Na parte inferior da janela de C#, clique na aba References.

        Clique com o botão direito, Add Reference from File... -> selecione Newtonsoft.Json.dll.

        Clique com o botão direito novamente, Add Reference from File... -> role para baixo e selecione System.dll.

        Clique com o botão direito novamente, Add Reference from File... -> selecione System.Text.RegularExpressions.dll.

        Agora você deve ter três arquivos na sua lista de referências.

    Clique no botão Compile na parte inferior. É OBRIGATÓRIO que apareça a mensagem "Compiled successfully!".

    Clique em OK.

Passo 4: Crie os Comandos do YouTube

    Vá para Commands.

    Clique com o botão direito, Add.

    Command: !translate. (Você pode iniciar uma nova linha e adicionar quantos comandos quiser).

    Location: Verifique se está definido como Start of message.

    Marque a caixa YouTube Message em "sources" e desmarque a caixa Twitch Message.

    Clique em OK.

    Repita esses passos para adicionar quaisquer outros nomes de comando que desejar (ex: !traduzir, !honyaku).

Passo 5: Crie o Comando de Ajuda !languages

(Opcional, mas altamente recomendado)

    Crie o Comando:

        Volte para Commands.

        Adicione um novo comando chamado !languages.

        Command: !languages. (Este comando é o que os usuários irão digitar).

        Action: Selecione a ação que você vai criar no próximo passo (ex: Explain Languages).

        Location: Verifique se está definido como Start of message.

        Marque a caixa YouTube Message em "sources" e desmarque a caixa Twitch Message.

        Clique em OK.

    Crie a Ação:

        Vá para a aba Actions, adicione uma nova ação. Nomeie como Explain Languages ou algo similar.

        Em Sub-Actions, adicione Core -> C# -> Execute C# Code.

        APAGUE todo o código na janela que aparecer.

        COLE o código explainlanguages.cs.

        Clique em OK.

        Em Triggers (Gatilhos), adicione Core -> Commands -> Command Triggered.

        Na lista suspensa, escolha o comando que você acabou de criar (!languages).

        Clique em OK.

Passo 6: Teste!

Observação: Como esta é uma IA, é natural que ocorram erros e coisas estranhas de vez em quando.
