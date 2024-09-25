//Ricardo, Adam, Erick
#define CG_Gizmo // debugar gráfico.
#define CG_OpenGL // render OpenGL.
// #define CG_DirectX // render DirectX.
// #define CG_Privado // código do professor.

using CG_Biblioteca;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace gcgcg
{
    public class Mundo : GameWindow
    {
        Objeto mundo;
        private char rotuloAtual = '?';
        private Objeto objetoSelecionado;
        private Circulo joystick;
        private Ponto centroJoystick;
        private Retangulo bBox;

        private const double Offset = 0.0005;

        private readonly float[] _sruEixos =
        {
            -0.5f, 0.0f, 0.0f, /* X- */ 0.5f, 0.0f, 0.0f, /* X+ */
            0.0f, -0.5f, 0.0f, /* Y- */ 0.0f, 0.5f, 0.0f, /* Y+ */
            0.0f, 0.0f, -0.5f, /* Z- */ 0.0f, 0.0f, 0.5f, /* Z+ */
        };

        private int _vertexBufferObject_sruEixos;
        private int _vertexArrayObject_sruEixos;

        private Shader _shaderVermelha;
        private Shader _shaderVerde;
        private Shader _shaderAzul;

        private bool _firstMove = true;
        private Vector2 _lastPos;

        public Mundo(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            mundo = new Objeto(null, ref rotuloAtual);
        }

        private void Diretivas()
        {
#if DEBUG
            Console.WriteLine("Debug version");
#endif
#if RELEASE
    Console.WriteLine("Release version");
#endif
#if CG_Gizmo
            Console.WriteLine("#define CG_Gizmo  // debugar gráfico.");
#endif
#if CG_OpenGL
            Console.WriteLine("#define CG_OpenGL // render OpenGL.");
#endif
#if CG_DirectX
      Console.WriteLine("#define CG_DirectX // render DirectX.");
#endif
#if CG_Privado
      Console.WriteLine("#define CG_Privado // código do professor.");
#endif
            Console.WriteLine("__________________________________ \n");
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            Diretivas();

            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            #region Eixos: SRU

            _vertexBufferObject_sruEixos = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject_sruEixos);
            GL.BufferData(BufferTarget.ArrayBuffer, _sruEixos.Length * sizeof(float), _sruEixos,
                BufferUsageHint.StaticDraw);
            _vertexArrayObject_sruEixos = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject_sruEixos);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            _shaderVermelha = new Shader("Shaders/shader.vert", "Shaders/shaderVermelha.frag");
            _shaderVerde = new Shader("Shaders/shader.vert", "Shaders/shaderVerde.frag");
            _shaderAzul = new Shader("Shaders/shader.vert", "Shaders/shaderAzul.frag");

            #endregion

            var pontoCentral = new Ponto4D(0.3, 0.3);

            // Ponto central
            centroJoystick = new Ponto(mundo, ref rotuloAtual, pontoCentral)
            {
                PrimitivaTamanho = 5,
                PrimitivaTipo = PrimitiveType.Points
            };

            // Joystick deslocado até o meio do ponto central
            joystick = new Circulo(mundo, ref rotuloAtual, 0.1, pontoCentral)
            {
                PrimitivaTipo = PrimitiveType.LineLoop
            };

            // Circulo maior deslocado até o meio do ponto central
            objetoSelecionado = new Circulo(mundo, ref rotuloAtual, 0.3, pontoCentral)
            {
                PrimitivaTipo = PrimitiveType.LineLoop
            };

            // Calcular posição do ponto simétrico do circulo de raio 0.3 no primeiro quadrante (45) 
            var pos = Matematica.GerarPtosCirculoSimetrico(0.3);

            var esquerdoInferior = new Ponto4D(pos, pos) + pontoCentral;
            var direitoSuperior = new Ponto4D(-pos, -pos) + pontoCentral;

            bBox = new Retangulo(mundo, ref rotuloAtual, esquerdoInferior, direitoSuperior)
            {
                PrimitivaTipo = PrimitiveType.LineLoop
            };
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);

#if CG_Gizmo
            Sru3D();
#endif
            mundo.Desenhar();
            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            #region Teclado

            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                Close();
            }
            else
            {
                if (KeyboardState.IsKeyDown(Keys.D) || KeyboardState.IsKeyDown(Keys.Right))
                {
                    Direita();
                }
                else if (KeyboardState.IsKeyDown(Keys.E) || KeyboardState.IsKeyDown(Keys.Left))
                {
                    Esquerda();
                }
                else if (KeyboardState.IsKeyDown(Keys.C) || KeyboardState.IsKeyDown(Keys.Up))
                {
                    Cima();
                }
                else if (KeyboardState.IsKeyDown(Keys.B) || KeyboardState.IsKeyDown(Keys.Down))
                {
                    Baixo();
                }
            }

            #endregion
        }
        
        private void AtualizarJoystick(Ponto4D pontoCentral)
        {
            // Círculo menor
            joystick.Atualizar(pontoCentral);
            
            // Ponto no meio do círculo menor
            centroJoystick.PontosAlterar(pontoCentral, 0);
            centroJoystick.Atualizar();
        }

        private void Cima()
        {
            var pontoCentral = centroJoystick.PontosId(0);
            
            // Deslocar o ponto central para cima pelo offset
            pontoCentral.Y += Offset;
            
            if (Matematica.Dentro(bBox.Bbox(), pontoCentral))
            {
                // Se estiver dentro da bbox:
                // Mostrar as linhas do retângulo
                // Atualizar os objetos com o ponto deslocado
                bBox.PrimitivaTipo = PrimitiveType.LineLoop;
                AtualizarJoystick(pontoCentral);
            }
            else
            {
                // Se não estiver dentro da bbox:
                // Mostrar os pontos do retângulo
                bBox.PrimitivaTipo = PrimitiveType.Points;
                
                // Calcular a disância entre o ponto deslocado e o centro do círculo maior.
                if (Matematica.DistanciaQuadrado(pontoCentral, new Ponto4D(0.3, 0.3)) <= 0.09)
                {
                    // Se estiver dentro, atualizar o joystick
                    AtualizarJoystick(pontoCentral);
                }
                else
                {
                    // Se não estiver dentro, voltar a posição anterior
                    pontoCentral.Y -= Offset;
                }
            }
        }

        private void Baixo()
        {
            var pontoCentral = centroJoystick.PontosId(0);
            
            // Deslocar o ponto central para baixo pelo offset
            pontoCentral.Y -= Offset;
            
            if (Matematica.Dentro(bBox.Bbox(), pontoCentral))
            {
                // Se estiver dentro da bbox:
                // Mostrar as linhas do retângulo
                // Atualizar os objetos com o ponto deslocado
                bBox.PrimitivaTipo = PrimitiveType.LineLoop;
                AtualizarJoystick(pontoCentral);
            }
            else
            {
                // Se não estiver dentro da bbox:
                // Mostrar os pontos do retângulo
                bBox.PrimitivaTipo = PrimitiveType.Points;
                
                // Calcular a disância entre o ponto deslocado e o centro do círculo maior.
                if (Matematica.DistanciaQuadrado(pontoCentral, new Ponto4D(0.3, 0.3)) <= 0.09)
                {
                    // Se estiver dentro, atualizar o joystick
                    AtualizarJoystick(pontoCentral);
                }
                else
                {
                    // Se não estiver dentro, voltar a posição anterior
                    pontoCentral.Y += Offset;
                }
            }
        }

        private void Direita()
        {
            var pontoCentral = centroJoystick.PontosId(0);
            
            // Deslocar o ponto central para direita pelo offset
            pontoCentral.X += Offset;
            
            if (Matematica.Dentro(bBox.Bbox(), pontoCentral))
            {
                // Se estiver dentro da bbox:
                // Mostrar as linhas do retângulo
                // Atualizar os objetos com o ponto deslocado
                bBox.PrimitivaTipo = PrimitiveType.LineLoop;
                AtualizarJoystick(pontoCentral);
            }
            else
            {
                // Se não estiver dentro da bbox:
                // Mostrar os pontos do retângulo
                bBox.PrimitivaTipo = PrimitiveType.Points;
                
                // Calcular a disância entre o ponto deslocado e o centro do círculo maior.
                if (Matematica.DistanciaQuadrado(pontoCentral, new Ponto4D(0.3, 0.3)) <= 0.09)
                {
                    // Se estiver dentro, atualizar o joystick
                    AtualizarJoystick(pontoCentral);
                }
                else
                {
                    // Se não estiver dentro, voltar a posição anterior
                    pontoCentral.X -= Offset;
                }
            }
        }

        private void Esquerda()
        {
            var pontoCentral = centroJoystick.PontosId(0);
            
            // Deslocar o ponto central para esquerda pelo offset
            pontoCentral.X -= Offset;
            
            if (Matematica.Dentro(bBox.Bbox(), pontoCentral))
            {
                // Se estiver dentro da bbox:
                // Mostrar as linhas do retângulo
                // Atualizar os objetos com o ponto deslocado
                bBox.PrimitivaTipo = PrimitiveType.LineLoop;
                AtualizarJoystick(pontoCentral);
            }
            else
            {
                // Se não estiver dentro da bbox:
                // Mostrar os pontos do retângulo
                bBox.PrimitivaTipo = PrimitiveType.Points;
                
                // Calcular a disância entre o ponto deslocado e o centro do círculo maior.
                if (Matematica.DistanciaQuadrado(pontoCentral, new Ponto4D(0.3, 0.3)) <= 0.09)
                {
                    // Se estiver dentro, atualizar o joystick
                    AtualizarJoystick(pontoCentral);
                }
                else
                {
                    // Se não estiver dentro, voltar a posição anterior
                    pontoCentral.X += Offset;
                }
            }
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Size.X, Size.Y);
        }

        protected override void OnUnload()
        {
            mundo.OnUnload();

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            GL.DeleteBuffer(_vertexBufferObject_sruEixos);
            GL.DeleteVertexArray(_vertexArrayObject_sruEixos);

            GL.DeleteProgram(_shaderVermelha.Handle);
            GL.DeleteProgram(_shaderVerde.Handle);
            GL.DeleteProgram(_shaderAzul.Handle);

            base.OnUnload();
        }

#if CG_Gizmo
        private void Sru3D()
        {
#if CG_OpenGL && !CG_DirectX
            GL.BindVertexArray(_vertexArrayObject_sruEixos);
            // EixoX
            _shaderVermelha.Use();
            GL.DrawArrays(PrimitiveType.Lines, 0, 2);
            // EixoY
            _shaderVerde.Use();
            GL.DrawArrays(PrimitiveType.Lines, 2, 2);
            // EixoZ
            _shaderAzul.Use();
            GL.DrawArrays(PrimitiveType.Lines, 4, 2);
#elif CG_DirectX && !CG_OpenGL
      Console.WriteLine(" .. Coloque aqui o seu código em DirectX");
#elif (CG_DirectX && CG_OpenGL) || (!CG_DirectX && !CG_OpenGL)
      Console.WriteLine(" .. ERRO de Render - escolha OpenGL ou DirectX !!");
#endif
        }
#endif
    }
}