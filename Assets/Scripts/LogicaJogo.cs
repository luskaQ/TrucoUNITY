using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LogicaJogo
{
    List<Carta>[] Ordem = new List<Carta>[13];
    int PontJogador = 0, PontIA = 0;
    public LogicaJogo()
    {
        for (int i = 0; i < Ordem.Length; i++)
        {
            Ordem[i] = new List<Carta>();
        }
    }
    public void setOrdem(Carta vira)
    {
        int aux_num = somaCarta(vira.valor);


        Ordem[12].Add(new Carta("clubs", aux_num));
        Ordem[11].Add(new Carta("hearts", aux_num));
        Ordem[10].Add(new Carta("spades", aux_num));
        Ordem[9].Add(new Carta("diamonds", aux_num));

        aux_num = 4;
        for (int i = 0; i < 9; i++)
        {
            if (aux_num == Ordem[12][0].valor)
            {
                aux_num = somaCarta(aux_num);
            }

            Ordem[i].Add(new Carta("clubs", aux_num));
            Ordem[i].Add(new Carta("hearts", aux_num));
            Ordem[i].Add(new Carta("spades", aux_num));
            Ordem[i].Add(new Carta("diamonds", aux_num));

            aux_num = somaCarta(aux_num);
        }



    }

    public int GanhaRodada(Carta minha, Carta adversario)
    {
        int ForcaMinha, ForcaAdversario;
        ForcaMinha = ForcaCarta(minha);
        ForcaAdversario = ForcaCarta(adversario);

        if (ForcaMinha == ForcaAdversario) return 0;
        if (ForcaMinha > ForcaAdversario) return 1;
        else return 2;
    }
    void Mao()
    {
        
    }

    static int somaCarta(int x)
    {
        if (x == 10) return 1;
        else return ++x;
    }
    public int ForcaCarta(Carta a)
    {
        for (int i = 0; i < 13; i++)
        {
            List<Carta> listaNesteNivel = Ordem[i];
            if (listaNesteNivel.Any(cartaDaLista => cartaDaLista.valor == a.valor && cartaDaLista.naipe == a.naipe))
            {
                return i;
            }
        }
        return -10;
    }
}
