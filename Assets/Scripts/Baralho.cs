using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using static LogicaJogo;
using TMPro;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement; // Adicione esta linha
using UnityEngine.UI;
public class Carta
{
    public string naipe;
    public int valor;

    public Carta(string naipe, int valor)
    {
        this.naipe = naipe;
        this.valor = valor;
    }
    public override string ToString()
    {
        return $"{valor} de {naipe}";
    }

}
public class Baralho : MonoBehaviour // : MonoBehaviour faz herdar a classe da unity, fazendo a classe poder interagir com o ambiente de jogo
{
    public static Baralho instance;
    public Texture2D[] texturasCartas;
    public Stack<Carta> pilha_cartas_mesa = new Stack<Carta>();
    public List<Carta> mao_1 = new List<Carta>();
    public List<Carta> mao_2 = new List<Carta>();
    public List<Carta> cartas = new List<Carta>();
    public Transform posicaoMao1;
    public Transform posicaoMao2;
    public float espacamentoEntreCartas = 1.1f;

    public GameObject cartaPrefab;

    public List<GameObject> cartasOponente_GAMESOBJ;
    public List<GameObject> cartasPlayer_GAMESOBJ;

    public Transform posicaoCartaNaMesa;
    public Transform posicaoCartaJogada1;
    public Transform posicaoCartaJogada2;

    public Carta vira;

    public LogicaJogo logicaJogo = new LogicaJogo();

    string[] naipes = { "spades", "hearts", "diamonds", "clubs" };

    public TMP_Text textoPlacarJogador;
    public TMP_Text textoPlacarOponente;

    public TMP_Text textoBotaoTruco;

    public TMP_Text texto_pedido_erguer;
    public TMP_Text texto_valorMao;

    public TMP_Text texto_turno_bot;

    public int pontuacaoJogador = 0;
    public int pontuacaoOponente = 0;
    public int valorMao = 1;
    public Carta cartaSelecionadaPlayer;
    public GameObject cartaSelecionadaPlayer_OBJ_JOGO;

    private Carta ultimaCartaJogada;
    private GameObject ultimaCartaJogada_GAME_OBJ;

    private Carta ultimaCartaJogadaBot;
    private GameObject ultimaCartaJogadaBot_GAME_OBJ;

    private static bool isTurnoPlayer = false;
    private bool playerJogou = false;
    private int maozada;
    private bool maior;
    private GameObject vira_GAME_OBJ;

    private int corte_truco_bot = 8;

    public Button botao_correr;
    public Button botao_aceitar;
    public TMP_Text texto_botao_aceitar;
    public TMP_Text texto_botao_correr;

    public Image imagem_botao_correr;
    public Image imagem_botao_aceitar;
    private bool trucou = false;
    private bool aguardandoRespostaTruco = false;
    private bool botTrucou = false;

    private bool jogadorPediuTruco = false;
    private bool BloquerTruco = false;


    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
        texturasCartas = Resources.LoadAll<Texture2D>("Frente");
    }
    void Start()
    {
        CarregarPlacar();
        StartCoroutine(GameLoop());
    }
    IEnumerator GameLoop()
    {
        while (pontuacaoJogador < 12 && pontuacaoOponente < 12)
        {
            textoBotaoTruco.enabled = true; 
            texto_turno_bot.enabled = false;
            desativar_botao_erguer();
            valorMao = 1;
            texto_valorMao.text = "Valor da mão: " + valorMao;
            maozada = 1;
            botTrucou = false;
            textoBotaoTruco.text = "TRUCO";
            embaralhar();
            distribuir();
            mostrarViraMesa();
            MostrarCartasJogadores();
            logicaJogo.setOrdem(vira);
            atualizarPlacar();

            bool playerComeca = Random.Range(0, 2) == 0;
            int pont_player_mao = 0;
            int pont_bot_mao = 0;
            maior = false;


            for (maozada = 1; maozada <= 3; maozada++)
            {
                Debug.Log($"Iniciando mãozada {maozada}");
                playerJogou = false;
                jogadorPediuTruco = false;

                if (playerComeca)
                {
                    yield return StartCoroutine(TurnoJogador());

                    if (jogadorPediuTruco)
                    {
                        yield return StartCoroutine(processoTrucoJogador());

                        Debug.Log("Truco resolvido. Vez do jogador jogar a carta obrigatória.");
                        yield return StartCoroutine(TurnoJogador());
                    }

                    yield return StartCoroutine(acaoBot());
                }
                else
                {
                    yield return StartCoroutine(acaoBot());

                    if (pontuacaoOponente >= 12 || pontuacaoJogador >= 12) yield break;

                    yield return StartCoroutine(TurnoJogador());

                    if (jogadorPediuTruco)
                    {
                        yield return StartCoroutine(processoTrucoJogador());
                        yield return StartCoroutine(TurnoJogador());
                    }
                }


                if (ultimaCartaJogada != null && ultimaCartaJogadaBot != null)
                {
                    int resultadoDaMaozada = logicaJogo.GanhaRodada(ultimaCartaJogada, ultimaCartaJogadaBot);

                    if (maior == true)
                    {
                        if (resultadoDaMaozada == 1) { pont_player_mao++; break; }
                        if (resultadoDaMaozada == 2) { pont_bot_mao++; break; }

                    }

                    if (resultadoDaMaozada == 0 && maozada == 1)
                    {
                        Debug.Log("Empachou na primeira");
                        maior = true;
                    }
                    else if (resultadoDaMaozada == 1)
                    {
                        Debug.Log("Player ganhou a maozada");
                        pont_player_mao++;
                        playerComeca = true;
                    }
                    else if (resultadoDaMaozada == 2)
                    {
                        Debug.Log("Bot ganhou a maozada");
                        pont_bot_mao++;
                        playerComeca = false;
                    }
                }

                yield return new WaitForSeconds(2);
                if (ultimaCartaJogada_GAME_OBJ != null) Destroy(ultimaCartaJogada_GAME_OBJ);
                if (ultimaCartaJogadaBot_GAME_OBJ != null) Destroy(ultimaCartaJogadaBot_GAME_OBJ);
                ultimaCartaJogada = null;
                ultimaCartaJogadaBot = null;

                if (pont_player_mao == 2 || pont_bot_mao == 2)
                {
                    break;
                }
            }


            Debug.Log($"FIM DA RODADA! Placar da rodada: Player {pont_player_mao} x {pont_bot_mao} Bot");
            if (pont_bot_mao > pont_player_mao)
            {
                pontuacaoOponente += valorMao;
            }
            else if (pont_player_mao > pont_bot_mao)
            {
                pontuacaoJogador += valorMao;
            }
            else
            {
                Debug.Log("Rodada empatada! Ninguém marca pontos.");
            }

            atualizarPlacar();
            SalvarPlacar();
            yield return StartCoroutine(limparMesaParaNovaRodada());
            yield return new WaitForSeconds(2);
        }

        Debug.Log("FIM DE JOGO! Reiniciando...");
        pontuacaoJogador = 0;
        pontuacaoOponente = 0;
        SalvarPlacar();
        ReiniciarJogo();
    }
    public void ReiniciarJogo()
    {
        int cenaAtualIndex = SceneManager.GetActiveScene().buildIndex;

        Debug.Log($"Recarregando a cena de índice: {cenaAtualIndex}");
        SceneManager.LoadScene(cenaAtualIndex);
    }
    public void SalvarPlacar()
    {
        PlayerPrefs.SetInt("PlacarJogador", pontuacaoJogador);

        PlayerPrefs.SetInt("PlacarOponente", pontuacaoOponente);

        PlayerPrefs.Save();

        Debug.Log($"Placar Salvo! Jogador: {pontuacaoJogador}, Oponente: {pontuacaoOponente}");
    }

    public void CarregarPlacar()
    {
        if (PlayerPrefs.HasKey("PlacarJogador"))
        {
            pontuacaoJogador = PlayerPrefs.GetInt("PlacarJogador");
        }

        if (PlayerPrefs.HasKey("PlacarOponente"))
        {
            pontuacaoOponente = PlayerPrefs.GetInt("PlacarOponente");
        }

        Debug.Log($"Placar Carregado! Jogador: {pontuacaoJogador}, Oponente: {pontuacaoOponente}");

        atualizarPlacar();
    }
    IEnumerator limparMesaParaNovaRodada()
    {
        Debug.Log("limpando a mesa para a próxima rodada");

        if (vira_GAME_OBJ != null) Destroy(vira_GAME_OBJ);

        foreach (GameObject cartaObj in cartasPlayer_GAMESOBJ)
        {
            if (cartaObj != null)
            {
                cartaObj.SetActive(false);
                CartaClicavel clicavel = cartaObj.GetComponent<CartaClicavel>();
                if (clicavel != null)
                {
                    clicavel.enabled = false;  // Desativa o IPointerDownHandler
                }
                Destroy(cartaObj);
            }
        }
        foreach (GameObject cartaObj in cartasOponente_GAMESOBJ)
        {
            if (cartaObj != null) { cartaObj.SetActive(false); Destroy(cartaObj); }
        }

        mao_1.Clear();
        mao_2.Clear();
        cartasPlayer_GAMESOBJ.Clear();
        cartasOponente_GAMESOBJ.Clear();

        yield return new WaitForEndOfFrame();

        Debug.Log("Mesa oficialmente limpa.");
    }
    void embaralhar()
    {
        cartas.Clear();
        foreach (string naipe in naipes)
        {
            for (int i = 1; i <= 10; i++)
            {
                cartas.Add(new Carta(naipe, i));
            }
        }

        for (int i = 0; i < cartas.Count; i++)
        {
            int indice_aleatorio = Random.Range(0, cartas.Count);
            Carta aux = cartas[i];
            cartas[i] = cartas[indice_aleatorio];
            cartas[indice_aleatorio] = aux;
        }
        pilha_cartas_mesa.Clear();
        foreach (Carta carta in cartas)
        {
            pilha_cartas_mesa.Push(carta);
        }
    }
    public void novoJogo()
    {
        pontuacaoJogador = 0;
        pontuacaoOponente = 0;
        SalvarPlacar();
        ReiniciarJogo();
    }
    IEnumerator TurnoJogador()
    {
        isTurnoPlayer = true;
        playerJogou = false;
        jogadorPediuTruco = false;

        Debug.Log("Turno do jogador. Aguardando ação (jogar carta ou trucar)...");

        while (!playerJogou && !jogadorPediuTruco)
        {
            yield return null;
        }

        if (playerJogou)
        {
            Debug.Log("Jogador jogou a carta: " + ultimaCartaJogada.ToString());
        }
        else if (jogadorPediuTruco)
        {
            Debug.Log("Jogador pediu truco. Ação de jogar carta foi interrompida para resolver o truco.");
        }

        isTurnoPlayer = false;
    }

    void distribuir()
    {
        Carta aux;
        for (int i = 0; i < 3; i++)
        {
            aux = pilha_cartas_mesa.Pop();
            mao_1.Add(aux);
            aux = pilha_cartas_mesa.Pop();
            mao_2.Add(aux);

            Debug.Log("i:" + i + " carta: " + aux.ToString());
        }
        aux = pilha_cartas_mesa.Pop();
        vira = aux;
    }

    void MostrarCartasJogadores()
    {
        Quaternion rotacaoParaCima = Quaternion.Euler(140f, 0, 0);
        for (int i = 0; i < mao_1.Count; i++)
        {
            Vector3 posicaoDaCarta = posicaoMao1.position + new Vector3(espacamentoEntreCartas * i, 0, 0);

            GameObject cartaObj = Instantiate(cartaPrefab, posicaoDaCarta, rotacaoParaCima);

            cartasPlayer_GAMESOBJ.Add(cartaObj);
            CartaClicavel clicavel = cartaObj.GetComponent<CartaClicavel>();

            if (clicavel != null)
            {
                clicavel.cartaData = mao_1[i];
                clicavel.isClicavel = true;
            }

            cartaObj.name = mao_1[i].ToString();
            CartaVisual visual = cartaObj.GetComponent<CartaVisual>();
            Texture2D textura = ObterTextura(mao_1[i]);

            visual.AplicarTextura(textura);
            visual.MostrarFrente();
        }

        for (int i = 0; i < mao_2.Count; i++)
        {
            Vector3 posicaoDaCarta = posicaoMao2.position + new Vector3(espacamentoEntreCartas * i, 0, 0);
            GameObject cartaObj = Instantiate(cartaPrefab, posicaoDaCarta, posicaoMao2.rotation);

            cartasOponente_GAMESOBJ.Add(cartaObj);


            CartaClicavel clicavelOponente = cartaObj.GetComponent<CartaClicavel>();
            if (clicavelOponente != null)
            {
                clicavelOponente.isClicavel = false;
            }

            cartaObj.name = mao_2[i].ToString();
            CartaVisual visual = cartaObj.GetComponent<CartaVisual>();
            Texture2D textura = ObterTextura(mao_2[i]);

            visual.AplicarTextura(textura);
            visual.MostrarVerso();
        }
    }

    public void mostrarViraMesa()
    {
        Quaternion rotacaoParaCima = Quaternion.Euler(180f, 0, 0);

        vira_GAME_OBJ = Instantiate(cartaPrefab, posicaoCartaNaMesa.position, rotacaoParaCima);
        vira_GAME_OBJ.name = vira.ToString();
        CartaClicavel clicavelVira = vira_GAME_OBJ.GetComponent<CartaClicavel>();
        if (clicavelVira != null)
        {
            clicavelVira.isClicavel = false;
        }

        CartaVisual visual = vira_GAME_OBJ.GetComponent<CartaVisual>();
        Texture2D textura = ObterTextura(vira);
        visual.AplicarTextura(textura);
        visual.MostrarFrente(); // Mostra a face da carta
    }

    Texture2D ObterTextura(Carta carta)
    {
        string nomeArquivo = $"card_{carta.naipe.ToLower()}_{carta.valor}";
        foreach (Texture2D textura in texturasCartas)
        {
            if (textura.name.ToLower() == nomeArquivo)
                return textura;
        }

        //Debug.LogWarning($"Textura não encontrada para: {nomeArquivo}");
        return null;
    }

    private void atualizarPlacar()
    {
        //Debug.Log("AAAAAAAAAAAAA");
        textoPlacarJogador.text = "Nós: " + pontuacaoJogador;
        textoPlacarOponente.text = "Eles: " + pontuacaoOponente;
    }
    public void botaoTrucoClicado()
    {
        if (isTurnoPlayer && !jogadorPediuTruco)
        {
            jogadorPediuTruco = true;
            textoBotaoTruco.enabled = false;
        }
        else
        {
            Debug.Log("Não é possível trucar agora.");
        }
    }

    IEnumerator processoTrucoJogador()
    {
        jogadorPediuTruco = true;

        yield return null;

        Debug.Log("Jogador pediu TRUCO! Aguardando resposta do bot...");
        yield return new WaitForSeconds(1.5f); // Bot "pensa"

        bool botAceita = false;
        foreach (var carta in mao_2)
        {
            if (logicaJogo.ForcaCarta(carta) >= 5)
            {
                botAceita = true;
                break;
            }
        }

        if (botAceita)
        {
            Debug.Log("Bot ACEITOU o truco!");
            aumentarValorMao();
        }
        else
        {
            Debug.Log("Bot CORREU do truco!");
            pontuacaoJogador += valorMao;
            atualizarPlacar();
            SalvarPlacar();
            ReiniciarJogo();
        }
    }

    public void jogarCartaClicadaPlayer(CartaClicavel cartaClicada)
    {
        Debug.Log("Click detectado");
        if (isTurnoPlayer)
        {
            if (ultimaCartaJogada != null)
            {
                Destroy(ultimaCartaJogada_GAME_OBJ);
            }

            Debug.Log("Carta clicada: " + cartaClicada.cartaData.ToString());

            cartaSelecionadaPlayer = cartaClicada.cartaData;
            cartaSelecionadaPlayer_OBJ_JOGO = cartaClicada.gameObject;

            cartaSelecionadaPlayer_OBJ_JOGO.transform.position = new Vector3(-1.87f, 3.26f, -7.14f);
            cartaSelecionadaPlayer_OBJ_JOGO.transform.rotation = Quaternion.Euler(180f, 0, 0);


            mao_1.Remove(cartaSelecionadaPlayer);

            CartaClicavel clicavel = cartaSelecionadaPlayer_OBJ_JOGO.GetComponent<CartaClicavel>();
            if (clicavel != null)
            {
                clicavel.isClicavel = false;
            }

            ultimaCartaJogada = cartaSelecionadaPlayer;
            ultimaCartaJogada_GAME_OBJ = cartaSelecionadaPlayer_OBJ_JOGO;

            Debug.Log("DEFININDO PLAYER JOGOU COMO TRUE");
            playerJogou = true;
        }
        else
        {
            Debug.Log("Nao é seu turno!");
        }
    }

    public void jogarCartaJogadaBOT(Carta cartaOponente, GameObject cartaOponente_GAMEOBJ)
    {
        if (ultimaCartaJogadaBot != null)
        {
            Destroy(ultimaCartaJogadaBot_GAME_OBJ);
        }
        mao_2.Remove(cartaOponente);
        cartasOponente_GAMESOBJ.Remove(cartaOponente_GAMEOBJ);

        Debug.Log("Carta jogada BOT: " + cartaOponente.ToString());

        cartaOponente_GAMEOBJ.transform.position = new Vector3(2.3f, 3.25f, -5.55f);
        cartaOponente_GAMEOBJ.transform.rotation = Quaternion.Euler(180f, 0f, 0);

        ultimaCartaJogadaBot = cartaOponente;
        ultimaCartaJogadaBot_GAME_OBJ = cartaOponente_GAMEOBJ;
    }

    public static void setIsTurnoPlayer(bool _isTurnoPlayer)
    {
        isTurnoPlayer = _isTurnoPlayer;
    }

    public IEnumerator acaoBot()
    {
        texto_turno_bot.enabled = true;
        yield return new WaitForSeconds(2);

        if (Random.Range(1, 11) >= corte_truco_bot && valorMao < 12 && !botTrucou)
        {
            ativar_botao_erguer();
            trucou = true;
            aguardandoRespostaTruco = true;
            while (aguardandoRespostaTruco)
            {
                /////////////////////////////////////////////////////////////////
                //isTurnoPlayer = false;//LEMBRAR QUE ESTA LINHA PODE SER PERIGOSA/
                /////////////////////////////////////////////////////////////////
                yield return null;
            }
            textoBotaoTruco.enabled = true;
        }
        List<int> forcaCartas = new List<int>();
        forcaCartas.Clear();
        int cartaBot = 0;
        foreach (Carta carta in mao_2)
        {
            forcaCartas.Add(logicaJogo.ForcaCarta(carta));
        }
        forcaCartas.Sort();
        if (maozada == 1)
        {
            if (!playerJogou)
            {

                for (int i = 0; i < mao_2.Count; i++)
                {
                    if (logicaJogo.ForcaCarta(mao_2[i]) == forcaCartas.Last())
                    {
                        jogarCartaJogadaBOT(mao_2[i], cartasOponente_GAMESOBJ[i]);
                        break;
                    }
                }
            }
            else
            {
                if (ultimaCartaJogada != null)
                {
                    for (int i = 0; i < mao_2.Count; i++)
                    {
                        if (logicaJogo.ForcaCarta(mao_2[i]) > logicaJogo.ForcaCarta(ultimaCartaJogada))
                        {
                            cartaBot = i;
                            break;
                        }
                        else if (logicaJogo.ForcaCarta(mao_2[i]) == logicaJogo.ForcaCarta(ultimaCartaJogada))
                        {
                            cartaBot = i;
                            break;
                        }
                        else
                        {
                            cartaBot = i;
                        }
                    }
                    jogarCartaJogadaBOT(mao_2[cartaBot], cartasOponente_GAMESOBJ[cartaBot]);
                }

            }
        }
        else if (maozada == 2 && maior)
        {
            for (int i = 0; i < mao_2.Count; i++)
            {
                if (logicaJogo.ForcaCarta(mao_2[i]) == forcaCartas.Last())
                {
                    jogarCartaJogadaBOT(mao_2[i], cartasOponente_GAMESOBJ[i]);
                    break;
                }
            }
        }
        else if (maozada == 2 && !maior)
        {
            if (!playerJogou)
            {

                for (int i = 0; i < mao_2.Count; i++)
                {
                    if (logicaJogo.ForcaCarta(mao_2[i]) == forcaCartas.Last())
                    {
                        jogarCartaJogadaBOT(mao_2[i], cartasOponente_GAMESOBJ[i]);
                        break;
                    }
                }
            }
            else
            {
                if (ultimaCartaJogada != null)
                {
                    for (int i = 0; i < mao_2.Count; i++)
                    {
                        if (logicaJogo.ForcaCarta(mao_2[i]) > logicaJogo.ForcaCarta(ultimaCartaJogada))
                        {
                            cartaBot = i;
                            break;
                        }
                        else if (logicaJogo.ForcaCarta(mao_2[i]) == logicaJogo.ForcaCarta(ultimaCartaJogada))
                        {
                            cartaBot = i;
                            break;
                        }
                        else
                        {
                            cartaBot = i;
                        }
                    }
                    jogarCartaJogadaBOT(mao_2[cartaBot], cartasOponente_GAMESOBJ[cartaBot]);
                }

            }
        }
        else if (maozada == 3)
        {
            if (!playerJogou)
            {

                for (int i = 0; i < mao_2.Count; i++)
                {
                    if (logicaJogo.ForcaCarta(mao_2[i]) == forcaCartas.Last())
                    {
                        jogarCartaJogadaBOT(mao_2[i], cartasOponente_GAMESOBJ[i]);
                        break;
                    }
                }
            }
            else
            {
                if (ultimaCartaJogada != null)
                {
                    for (int i = 0; i < mao_2.Count; i++)
                    {
                        if (logicaJogo.ForcaCarta(mao_2[i]) > logicaJogo.ForcaCarta(ultimaCartaJogada))
                        {
                            cartaBot = i;
                            break;
                        }
                        else if (logicaJogo.ForcaCarta(mao_2[i]) == logicaJogo.ForcaCarta(ultimaCartaJogada))
                        {
                            cartaBot = i;
                            break;
                        }
                        else
                        {
                            cartaBot = i;
                        }
                    }
                    jogarCartaJogadaBOT(mao_2[cartaBot], cartasOponente_GAMESOBJ[cartaBot]);
                }

            }
        }
        texto_turno_bot.enabled = false;
    }


    public void aumentarValorMao()
    {
        if (valorMao == 1)
        {
            valorMao = 3;
        }
        else
        {
            valorMao += 3;
        }
        texto_valorMao.text = "Valor da mão: " + valorMao;

        if (valorMao == 3)
        {
            texto_pedido_erguer.text = "Oponente pediu seis";
        }
        else if (valorMao == 6)
        {
            texto_pedido_erguer.text = "Oponente pediu nove";
        }
        else if (valorMao == 9)
        {
            texto_pedido_erguer.text = "Oponente pediu doze";
        }

        if (textoBotaoTruco.text == "TRUCO")
        {
            Debug.Log("TRUCOOOO!");
            textoBotaoTruco.text = "SEIS";
        }
        else if (textoBotaoTruco.text == "SEIS")
        {
            Debug.Log("SEISSS!");
            textoBotaoTruco.text = "NOVE";
        }
        else if (textoBotaoTruco.text == "NOVE")
        {
            Debug.Log("NOVEEEE!");
            textoBotaoTruco.text = "DOZE";
        }
        else if (textoBotaoTruco.text == "DOZE")
        {
            Debug.Log("DOZEE!");
            textoBotaoTruco.text = " ";
        }
        else
        {
            Debug.Log("Nao é possivel aumentar mais");
        }

    }

    public void desativar_botao_erguer()
    {
        texto_pedido_erguer.enabled = false;
        botao_aceitar.enabled = false;
        botao_correr.enabled = false;
        texto_botao_aceitar.enabled = false;
        texto_botao_correr.enabled = false;
        imagem_botao_correr.enabled = false;
        imagem_botao_aceitar.enabled = false;
    }
    public void ativar_botao_erguer()
    {
        texto_pedido_erguer.enabled = true;
        botao_aceitar.enabled = true;
        botao_correr.enabled = true;
        texto_botao_aceitar.enabled = true;
        texto_botao_correr.enabled = true;
        imagem_botao_correr.enabled = true;
        imagem_botao_aceitar.enabled = true;


    }
    public void aceitar_erguida()
    {
        aumentarValorMao();
        desativar_botao_erguer();
        aguardandoRespostaTruco = false;
    }
    public void recusar_erguida()
    {
        desativar_botao_erguer();
        aguardandoRespostaTruco = false;
        pontuacaoOponente += valorMao;
        SalvarPlacar();
        ReiniciarJogo();
    }
}



