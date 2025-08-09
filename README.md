# Translation Bot
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

    Fully Configurable: Almost all settings, languages, and bot responses are controlled by easy-to-edit .json files. 
    No need to touch the C# code to make changes!

    User Profiles: Viewers can set their own default language and bot personality using the !sl command.

    Multi-Platform: Separate, optimized code is provided for both Twitch and YouTube.

    Directs users to a comprehensive help guide via the !langhelp command.
    
Guide to Translation Bot:
    
    Download streamer.bot https://streamer.bot/

    In Streamer.bot, go to Platforms -> YouTube -> Accounts.

    Sign in with the Google Account that is the Owner or Moderator of your YouTube channel.

    When Google asks for permissions, CHECK ALL THE BOXES. 
    You must grant it permission to "manage your YouTube account" and send messages.

    Do practically the same for Twitch if you are using the same for that.

Step 1: Get Your Google Cloud/Gemini API Key

    Go to Google AI Studio: aistudio.google.com and sign in.

    Click "Get API key" on the left, then "Create API key in new project".

    Copy your new API key and paste it into Notepad for now.

    Now, we must enable the API in the cloud console. Go to: console.cloud.google.com.

    Make sure the project selected at the top matches the one you just created.

    In the search bar, type Generative Language API and click on it.

    Click the ENABLE button.

    The Credit Card Step: If it prompts you to enable billing by adding a credit card, you will need to do so. 
    Don't panic! This is primarily for identity verification and for if you go way over the free limits. 
    The bot we built has a safety limiter so you will not be charged.

 Step 2: Download & Place the Configuration Files
 
       This bot runs on external .json files. This makes it easy to customize without editing code.

        Download the MyBotFiles folder and unzip it. 
        
        It contains three essential .json files: 
        
        translation_config.json (Contains all language, tone, and replacement maps)

        translation_templates.json (Contains all bot response messages)

        translation_user_profiles.json (This will store your viewers' settings and will be empty)

Step 3: Set Up Global Variables in Streamer.bot

    In Streamer.bot, go to Variables and right click to add a new global variable.

  Add the API Key:

        Right-click, Add.

        Name: geminiApiKey (case-sensitive!)

        Value: Paste your API key here.

        Click OK.

  Add the Slur Filter Blocklist (If you don't want to block any words then skip this step entirely):

        Right-click, Add.

        Name: translateBlocklist

        Value: Enter a comma-separated list of words to block (e.g., word1,word2,word3). 
        You can find public lists on GitHub by searching "streamer blocklist". 
        
        Click OK.

   TranslateEnabled:

        Type: Boolean

        Value: Set to true

        Click OK.

  translateUserBlocklist

       Type: String

       Value: (Optional) A comma-separated list of usernames to block from using the commands (e.g., user1,user2).
    
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


  Step 4: Import the Commands:

   Copy the Import String: 
    
    Copy the giant block of text from the translation bot file.

    Import into Streamer.bot 
    
    In Streamer.bot, go to the top tabs, and choose Import. Paste the text there and click 'Import'.

    This will automatically create all the necessary Actions and Commands for you.
  
  If you want to make it for one platform instead of both, simply disable every command/action for the one you don't want. 
    
Note: As this is an AI there are bound to be mistakes and odd things of the sort at times.


-----------------------------

# Translation Bot

Funcionalidades do Bot:

     Traduz mensagens de outros idiomas para o inglês por padrão.

     Traduz mensagens do inglês para o português do Brasil por padrão.

     Pode traduzir para outros idiomas por comando (ex: !translate es olá).

     Inclui "idiomas" divertidos de brincadeira como Simlish, Código Morse e mais.

     Possui um filtro de entrada manual para palavras que você deseja bloquear.
    
     Inclui um limitador de requisições para evitar que você seja cobrado pelo uso da API.

     Lida de forma inteligente com idiomas com gênero usando marcadores opcionais como %ela/dela% e *NomePróprio*.
    
     Consegue lidar com múltiplos tons em uma única frase (ex: &sarcástico& ... &sério& ...).

     Divide automaticamente mensagens longas que, de outra forma, falhariam ao serem enviadas.

     Totalmente Configurável: Quase todas as configurações, idiomas e respostas do bot são controladas por arquivos .json fáceis de editar.
    Não é preciso tocar no código C# para fazer alterações!

     Perfis de Usuário: Os espectadores podem definir seu próprio idioma padrão e a personalidade do bot usando o comando !sl.

     Multiplataforma: Código separado e otimizado é fornecido tanto para a Twitch quanto para o YouTube.

     Direciona os usuários para um guia de ajuda completo através do comando !langhelp.

Guia do Translation Bot:

    No Streamer.bot, vá para Platforms -> YouTube -> Accounts.
    
    Faça login com a conta do Google que é Proprietária (Owner) ou Moderadora do seu canal do YouTube.
    
    Quando o Google solicitar permissões, MARQUE TODAS AS CAIXAS.
    Você deve conceder permissão para "gerenciar sua conta do YouTube" e enviar mensagens.
    
    Faça praticamente o mesmo para a Twitch, se for usar o bot lá também.
  
  Passo 1: Obtenha sua Chave de API do Google Cloud/Gemini
    
    Acesse o Google AI Studio: aistudio.google.com e faça login.
    
    Clique em "Get API key" (Obter chave de API) à esquerda, e depois em "Create API key in new project" (Criar chave de API em novo projeto).
    
    Copie sua nova chave de API e cole-a no Bloco de Notas por enquanto.
    
    Agora, precisamos ativar a API no console da nuvem. Acesse: console.cloud.google.com.
    
    Certifique-se de que o projeto selecionado no topo corresponde ao que você acabou de criar.
    
    Na barra de pesquisa, digite `Generative Language API` e clique nela.
    
    Clique no botão ATIVAR (ENABLE).
    
    **O Passo do Cartão de Crédito:** Se o sistema pedir para você ativar o faturamento adicionando um cartão de crédito, será necessário fazê-lo.
    Não se assuste! Isso é principalmente para verificação de identidade e para casos em que você ultrapasse muito os limites gratuitos.
    O bot que criamos possui um limitador de segurança para que você não seja cobrado.

Passo 2: Baixe e Posicione os Arquivos de Configuração

    Este bot funciona com base em arquivos .json externos. Isso facilita a personalização sem precisar editar o código.

    Baixe a pasta `MyBotFiles` e descompacte-a.

    Ela contém três arquivos .json essenciais:

    `translation_config.json` (Contém todos os mapas de idioma, tom e substituição)

    `translation_templates.json` (Contém todas as mensagens de resposta do bot)

    `translation_user_profiles.json` (Este arquivo armazenará as configurações dos seus espectadores e estará vazio)

Passo 3: Configure as Variáveis Globais no Streamer.bot

    No Streamer.bot, vá para a aba "Variables" e clique com o botão direito para adicionar uma nova variável global.
    
  Adicione a Chave de API:
    
    Clique com o botão direito, Add (Adicionar).
    
    Name (Nome): `geminiApiKey` (sensível a maiúsculas e minúsculas!)
    
    Value (Valor): Cole sua chave de API aqui.
    
    Clique em OK.

    Adicione a Lista de Bloqueio de Palavras Ofensivas (Se você não quiser bloquear nenhuma palavra, pule este passo completamente):
    
    Clique com o botão direito, Add (Adicionar).
    
    Name (Nome): `translateBlocklist`
    
    Value (Valor): Insira uma lista de palavras a serem bloqueadas, separadas por vírgula (ex: palavra1,palavra2,palavra3).
    Você pode encontrar listas públicas no GitHub pesquisando por "streamer blocklist".
    
    Clique em OK.

TranslateEnabled:

    Type (Tipo): `Boolean`
    
    Value (Valor): Defina como `true`
    
    Clique em OK.
    
    translateUserBlocklist:
    
    Type (Tipo): `String`
    
    Value (Valor): (Opcional) Uma lista de nomes de usuário, separados por vírgula, para bloquear o uso dos comandos (ex: usuario1,usuario2).

Adicione o Contador Diário:

  Clique com o botão direito, Add (Adicionar).

    Name (Nome): `geminiRequestCountDaily`
    
    Value (Valor): `0`
    
    Clique em OK.
    
    Adicione o Rastreador do Limite de Requisições:

 Clique com o botão direito, Add (Adicionar).

    Name (Nome): `geminiRequestTimestamps`
    
    Value (Valor): `0`
    
    Clique em OK.

Passo 4: Importe os Comandos:

    Copie o Código de Importação:
    
    Copie o bloco de texto gigante do arquivo do bot de tradução.
    
    Importe para o Streamer.bot
    
    No Streamer.bot, vá para as abas superiores e escolha "Import" (Importar). Cole o texto lá e clique em "Import".
    
    Isso criará automaticamente todas as Ações (Actions) e Comandos (Commands) necessários para você.
    
    
Se você quiser usá-lo em apenas uma plataforma em vez de ambas, simplesmente desative todos os comandos/ações da plataforma que você não quer usar.   

Observação: Como esta é uma IA, é provável que ocorram erros e coisas estranhas do tipo de vez em quando.
