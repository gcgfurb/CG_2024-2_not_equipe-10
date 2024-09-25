//Ricardo, Adam, Erick
#define cg_debug
using CG_Biblioteca;
using System;
using OpenTK.Graphics.OpenGL4;

namespace gcgcg
{
    internal class SrPalito : Objeto
    {
        private double angulo = 45;
        private double raio = 0.5;

        private Ponto4D ptoIni = new(); //Pé (base)
        private Ponto4D ptoFim = new(); //Cabeça (topo)

        public SrPalito(Objeto paiRef, ref char _rotulo) : base(paiRef, ref _rotulo)
        {
            PrimitivaTipo = PrimitiveType.Lines;
            PrimitivaTamanho = 1f;

            PontosAdicionar(ptoIni);
            PontosAdicionar(ptoFim);

            Atualizar();
        }

        public void Atualizar()
        {
            //Substituir o ponto inicial pelo atual
            PontosAlterar(ptoIni, 0);

            //Gerar o ponto da cabeça com o ângulo e raio atual
            ptoFim = Matematica.GerarPtosCirculo(angulo, raio);

            //Atualizar para a coordenada X correta
            ptoFim.X += ptoIni.X;

            //Substituir o ponto final pelo novo gerado
            PontosAlterar(ptoFim, 1);
            ObjetoAtualizar();
        }

        public void AtualizarPe(double peInc)
        {
            //Criar um novo ponto com offset no X
            ptoIni = new Ponto4D(ptoIni.X + peInc, ptoIni.Y);

            Atualizar();
        }

        public void AtualizarRaio(double raioInc)
        {
            raio += raioInc;
            Atualizar();
        }

        public void AtualizarAngulo(double anguloInc)
        {
            angulo += anguloInc;
            Atualizar();
        }

#if cg_debug
        public override string ToString()
        {
            string retorno;
            retorno = "__ Objeto SrPalito _ tipo: " + PrimitivaTipo + " _ tamanho: " + PrimitivaTamanho + "\n";
            retorno += ImprimeToString();
            return retorno;
        }
#endif
    }
}