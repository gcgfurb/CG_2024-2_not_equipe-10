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
            raio = _raio;
            PrimitivaTipo = PrimitiveType.Points;
            PrimitivaTamanho = 5;

            for (var i = 0; i < 360; i += 5)
            {
                Ponto4D ponto = Matematica.GerarPtosCirculo(i, raio);
                PontosAdicionar(ponto + ptoDeslocamento); 
            }

            ObjetoAtualizar();
        }

        public void Atualizar(Ponto4D ptoDeslocamento)
        {
            var index = 0;
            for (var i = 0; i < 360; i += 5)
            {
                Ponto4D ponto = Matematica.GerarPtosCirculo(i, raio);
                PontosAlterar(ponto + ptoDeslocamento, index); 
                index++;
            }

            ObjetoAtualizar();
        }

#if CG_Debug
        public override string ToString()
        {
            string retorno;
            retorno = "__ Objeto Circulo _ Tipo: " + PrimitivaTipo + " _ Tamanho: " + PrimitivaTamanho + "\n";
            retorno += ImprimeToString();
            return retorno;
        }
#endif
    }
}