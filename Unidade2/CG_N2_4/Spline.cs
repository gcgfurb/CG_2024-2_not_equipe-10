//Ricardo, Adam, Erick
#define CG_Debug

using System;
using System.Collections.Generic;
using CG_Biblioteca;
using OpenTK.Graphics.OpenGL4;

namespace gcgcg
{
    internal class Spline : Objeto
    {
        private readonly Shader redShader = new("Shaders/shader.vert", "Shaders/shaderVermelha.frag");
        private readonly Shader whiteShader = new("Shaders/shader.vert", "Shaders/shaderBranca.frag");
        private readonly Shader cyanShader = new("Shaders/shader.vert", "Shaders/shaderCiano.frag");
        private char rotulo;

        private const int MaxPts = 10;
        private int ptsCount = 10;

        private int selectedIndex;

        private readonly Ponto[] controlPoints = new Ponto[4];
        private readonly double[,] bezierMatrix = new double[11, 4];
        
        private readonly Ponto4D[] originalPontos4D =
        {
            new(0.5, -0.5),
            new(0.5, 0.5),
            new(-0.5, 0.5),
            new(-0.5, -0.5)
        };
        
        private Poligono controlPoly;

        public Spline(Objeto paiRef, ref char _rotulo) : base(paiRef, ref _rotulo)
        {
            rotulo = _rotulo;
            PrimitivaTipo = PrimitiveType.LineStrip;

            SetupBasePoints();
            SetupPoliedroControle();
            CalculateBezierMatrix();

            for (var index = 0; index <= ptsCount; ++index)
            {
                PontosAdicionar(new Ponto4D());
            }

            Atualizar();
        }

        public void AtualizarSpline(Ponto4D ptoInc, bool proximo = false)
        {
            if (proximo)
            {
                selectedIndex = selectedIndex >= 3 ? 0 : ++selectedIndex;
            }

            controlPoints[selectedIndex].PontosAlterar(controlPoints[selectedIndex].PontosId(0) + ptoInc, 0);
            controlPoints[selectedIndex].ObjetoAtualizar();
            controlPoly.PontosAlterar(controlPoints[selectedIndex].PontosId(0), selectedIndex);
            controlPoly.ObjetoAtualizar();
            Atualizar();
        }

        public void SplineQtdPto(int inc)
        {
            switch (inc)
            {
                case < 0 when ptsCount > 0:
                case > 0 when ptsCount < MaxPts:
                    ptsCount = ptsCount += inc;
                    break;
            }

            CalculateBezierMatrix();
            Atualizar();
        }

        public void Reset()
        {
            for (var i = 0; i < controlPoints.Length; i++)
            {
                controlPoints[i].PontosAlterar(originalPontos4D[i], 0);
                controlPoints[i].ObjetoAtualizar();
                controlPoly.PontosAlterar(controlPoints[i].PontosId(0), i);
            }

            controlPoly.ObjetoAtualizar();

            CalculateBezierMatrix();
            Atualizar();
        }

        private void Atualizar()
        {
            var pontos4d = new Ponto4D[controlPoints.Length];
            for (var i = 0; i < controlPoints.Length; i++)
            {
                pontos4d[i] = new Ponto4D(controlPoints[i].PontosId(0));
                
                controlPoints[i].shaderCor = i == selectedIndex ? redShader : whiteShader;
            }

            for (var i = 0; i <= ptsCount; ++i)
            {
                PontosAlterar(
                    new Ponto4D(
                        x: pontos4d[0].X * bezierMatrix[i, 0] + pontos4d[1].X * bezierMatrix[i, 1] +
                           pontos4d[2].X * bezierMatrix[i, 2] + pontos4d[3].X * bezierMatrix[i, 3],
                        y: pontos4d[0].Y * bezierMatrix[i, 0] + pontos4d[1].Y * bezierMatrix[i, 1] +
                           pontos4d[2].Y * bezierMatrix[i, 2] + pontos4d[3].Y * bezierMatrix[i, 3]),
                    i);
            }

            ObjetoAtualizar();
        }

        private void SetupBasePoints()
        {
            for (var i = 0; i < controlPoints.Length; i++)
            {
                controlPoints[i] = new Ponto(this, ref rotulo, originalPontos4D[i]);
                FilhoAdicionar(controlPoints[i]);
                controlPoints[i].ObjetoAtualizar();
            }
        }

        private void SetupPoliedroControle()
        {
            var pontosPoligono = new List<Ponto4D>
            {
                controlPoints[0].PontosId(0),
                controlPoints[1].PontosId(0),
                controlPoints[2].PontosId(0),
                controlPoints[3].PontosId(0)
            };

            controlPoly = new Poligono(this, ref rotulo, pontosPoligono)
            {
                PrimitivaTipo = PrimitiveType.LineStrip,
                shaderCor = cyanShader
            };

            FilhoAdicionar(controlPoly);
            controlPoly.ObjetoAtualizar();
        }

        private void CalculateBezierMatrix()
        {
            var i = 0;

            for (var u = 0.0; u <= 1.0; u += 1.0 / ptsCount)
            {
                bezierMatrix[i, 0] = Math.Pow(1.0 - u, 3.0);
                bezierMatrix[i, 1] = 3.0 * u * Math.Pow(1.0 - u, 2.0);
                bezierMatrix[i, 2] = 3.0 * Math.Pow(u, 2.0) * (1.0 - u);
                bezierMatrix[i, 3] = Math.Pow(u, 3.0);
                i++;
            }
        }


#if CG_Debug
        public override string ToString()
        {
            string retorno;
            retorno = "__ Objeto Spline _ Tipo: " + PrimitivaTipo + " _ Tamanho: " + PrimitivaTamanho + "\n";
            retorno += ImprimeToString();
            return retorno;
        }
#endif
    }
}