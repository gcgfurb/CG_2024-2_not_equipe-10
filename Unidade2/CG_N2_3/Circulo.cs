//Ricardo, Adam, Erick
#define CG_Debug

using CG_Biblioteca;
using OpenTK.Graphics.OpenGL4;

namespace gcgcg
{
    internal class Circulo : Objeto
    {
        private readonly double raio;

        public Circulo(Objeto paiRef, ref char _rotulo, double _raio, Ponto4D ptoDeslocamento) : base(paiRef,
            ref _rotulo)
        {
            PrimitivaTipo = PrimitiveType.Points;
            PrimitivaTamanho = 5;
            raio = _raio;

            for (var i = 0; i < 360; i += 5)
            {
                Ponto4D ponto = Matematica.GerarPtosCirculo(i, raio);
                PontosAdicionar(ponto + ptoDeslocamento); //Considerar o deslocamento
            }

            ObjetoAtualizar();
        }

        public void Atualizar(Ponto4D ptoDeslocamento)
        {
            var index = 0;
            for (var i = 0; i < 360; i += 5)
            {
                Ponto4D ponto = Matematica.GerarPtosCirculo(i, raio);
                PontosAlterar(ponto + ptoDeslocamento, index); //Considerar o deslocamento
                index++;
            }

            ObjetoAtualizar();
        }

#if CG_Debug
        public override string ToString()
        {
            string retorno;
            retorno = "__ Objeto Circulo _ Tipo: " + PrimitivaTipo + " _ Tamanho: " + PrimitivaTamanho + "\n";
            retorno += base.ImprimeToString();
            return retorno;
        }
#endif
    }
}