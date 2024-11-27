/*
 As constantes dos pré-processors estão nos arquivos ".csproj"
 desse projeto e da CG_Biblioteca.
*/

#define CG_Gizmo // debugar gráfico.
#define CG_OpenGL // render OpenGL.

using CG_Biblioteca;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using System;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace gcgcg
{
    public class Mundo : GameWindow
    {
        Objeto mundo;
        private char rotuloNovo = '?';
        private Objeto objetoSelecionado;
        private Objeto objetoEmProgresso;

        private readonly float[] _sruEixos =
        {
            -0.5f, 0.0f, 0.0f, /* X- */ 0.5f, 0.0f, 0.0f, /* X+ */
            0.0f, -0.5f, 0.0f, /* Y- */ 0.0f, 0.5f, 0.0f, /* Y+ */
            0.0f, 0.0f, -0.5f, /* Z- */ 0.0f, 0.0f, 0.5f /* Z+ */
        };

        private int _vertexBufferObject_sruEixos;
        private int _vertexArrayObject_sruEixos;
        private int _vertexBufferObject_bbox;
        private int _vertexArrayObject_bbox;

        private Shader _shaderBranca;
        private Shader _shaderVermelha;
        private Shader _shaderVerde;
        private Shader _shaderAzul;
        private Shader _shaderCiano;
        private Shader _shaderMagenta;
        private Shader _shaderAmarela;

        public Mundo(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            mundo = new Objeto(null, ref rotuloNovo);
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            Utilitario.Diretivas();
#if CG_DEBUG      
    Console.WriteLine("Tamanho interno da janela de desenho: " + ClientSize.X + "x" + ClientSize.Y);
#endif

            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            #region Cores
            _shaderBranca = new Shader("Shaders/shader.vert", "Shaders/shaderBranca.frag");
            _shaderVermelha = new Shader("Shaders/shader.vert", "Shaders/shaderVermelha.frag");
            _shaderVerde = new Shader("Shaders/shader.vert", "Shaders/shaderVerde.frag");
            _shaderAzul = new Shader("Shaders/shader.vert", "Shaders/shaderAzul.frag");
            _shaderCiano = new Shader("Shaders/shader.vert", "Shaders/shaderCiano.frag");
            _shaderMagenta = new Shader("Shaders/shader.vert", "Shaders/shaderMagenta.frag");
            _shaderAmarela = new Shader("Shaders/shader.vert", "Shaders/shaderAmarela.frag");
            #endregion

#if CG_Gizmo
            #region Eixos: SRU  
            _vertexBufferObject_sruEixos = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject_sruEixos);
            GL.BufferData(BufferTarget.ArrayBuffer, _sruEixos.Length * sizeof(float), _sruEixos, BufferUsageHint.StaticDraw);
            _vertexArrayObject_sruEixos = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject_sruEixos);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            #endregion
#endif
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            mundo.Desenhar(new Transformacao4D());

#if CG_Gizmo
            Gizmo_Sru3D();
            Gizmo_BBox();
#endif
            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            #region Teclado

            if (KeyboardState.IsKeyDown(Keys.Escape))
                Close();
            else if (KeyboardState.IsKeyPressed(Keys.Space))
            {
                objetoSelecionado ??= mundo;
                // objetoSelecionado.shaderObjeto = _shaderBranca;
                objetoSelecionado = mundo.GrafocenaBuscaProximo(objetoSelecionado);
                // objetoSelecionado.shaderObjeto = _shaderAmarela;
            }
            else if (KeyboardState.IsKeyPressed(Keys.A))
                mundo.GrafocenaImprimir("");
            else if (KeyboardState.IsKeyPressed(Keys.P) && objetoSelecionado != null)
                Console.WriteLine(objetoSelecionado.ToString());
            else if (KeyboardState.IsKeyPressed(Keys.M) && objetoSelecionado != null)
                objetoSelecionado.MatrizImprimir();
            //TODO: não está atualizando a BBox com as transformações geométricas
            else if (KeyboardState.IsKeyPressed(Keys.I) && objetoSelecionado != null)
                objetoSelecionado.MatrizAtribuirIdentidade();
            else if (KeyboardState.IsKeyPressed(Keys.D) && objetoSelecionado != null)
            {
                objetoSelecionado.ObjetoRemover();
                // Remover a seleção do objeto (para não ter uma bbox fantasma)
                objetoSelecionado = mundo.GrafocenaBuscaProximo(mundo);
            }
            else if (KeyboardState.IsKeyPressed(Keys.V) && objetoSelecionado != null)
            {
                // Posição do mouse
                var mousePto = new Ponto4D(MousePosition.X, MousePosition.Y);
                // Ponto independente do tamanho da tela
                var sruPto = Utilitario.NDC_TelaSRU(Size.X, Size.Y, mousePto);
                
                var posicao = objetoSelecionado.IndexPontoMaisProximo(sruPto);
                if (posicao > -1)
                    objetoSelecionado.PontosAlterar(sruPto, posicao);
            }
            else if (KeyboardState.IsKeyPressed(Keys.E) && objetoSelecionado != null)
            {
                // Posição do mouse
                var mousePto = new Ponto4D(MousePosition.X, MousePosition.Y);
                // Ponto independente do tamanho da tela
                var sruPto = Utilitario.NDC_TelaSRU(Size.X, Size.Y, mousePto);
                
                var posicao = objetoSelecionado.IndexPontoMaisProximo(sruPto);
                objetoSelecionado.PontosRemover(posicao);
            }
            else if (KeyboardState.IsKeyPressed(Keys.R) && objetoSelecionado != null)
                objetoSelecionado.shaderObjeto = _shaderVermelha;
            else if (KeyboardState.IsKeyPressed(Keys.G) && objetoSelecionado != null)
                objetoSelecionado.shaderObjeto = _shaderVerde;
            else if (KeyboardState.IsKeyPressed(Keys.B) && objetoSelecionado != null)
                objetoSelecionado.shaderObjeto = _shaderAzul;
            else if (KeyboardState.IsKeyPressed(Keys.Left) && objetoSelecionado != null)
            {
                objetoSelecionado.MatrizTranslacaoXYZ(-0.05, 0, 0);

            }
            else if (KeyboardState.IsKeyPressed(Keys.Right) && objetoSelecionado != null)
            {
                objetoSelecionado.MatrizTranslacaoXYZ(0.05, 0, 0);
            }
            else if (KeyboardState.IsKeyPressed(Keys.Up) && objetoSelecionado != null)
            {
                objetoSelecionado.MatrizTranslacaoXYZ(0, 0.05, 0);

            }
            else if (KeyboardState.IsKeyPressed(Keys.Down) && objetoSelecionado != null)
            {
                objetoSelecionado.MatrizTranslacaoXYZ(0, -0.05, 0);

            }
            else if (KeyboardState.IsKeyPressed(Keys.PageUp) && objetoSelecionado != null)
                objetoSelecionado.MatrizEscalaXYZ(2, 2, 2);
            else if (KeyboardState.IsKeyPressed(Keys.PageDown) && objetoSelecionado != null)
                objetoSelecionado.MatrizEscalaXYZ(0.5, 0.5, 0.5);
            else if (KeyboardState.IsKeyPressed(Keys.Home) && objetoSelecionado != null)
                objetoSelecionado.MatrizEscalaXYZBBox(0.5, 0.5, 0.5);
            else if (KeyboardState.IsKeyPressed(Keys.End) && objetoSelecionado != null)
                objetoSelecionado.MatrizEscalaXYZBBox(2, 2, 2);
            else if (KeyboardState.IsKeyPressed(Keys.D1) && objetoSelecionado != null)
                objetoSelecionado.MatrizRotacao(10);
            else if (KeyboardState.IsKeyPressed(Keys.D2) && objetoSelecionado != null)
                objetoSelecionado.MatrizRotacao(-10);
            else if (KeyboardState.IsKeyPressed(Keys.D3) && objetoSelecionado != null)
                objetoSelecionado.MatrizRotacaoZBBox(10);
            else if (KeyboardState.IsKeyPressed(Keys.D4) && objetoSelecionado != null)
                objetoSelecionado.MatrizRotacaoZBBox(-10);
            else if (KeyboardState.IsKeyPressed(Keys.Enter)) // Finalizar objeto
            {
                mundo.FilhoAdicionar(objetoEmProgresso);
                objetoSelecionado = objetoEmProgresso;
                objetoEmProgresso = null;
            }

            #endregion

            #region Mouse

            if (MouseState.IsButtonPressed(MouseButton.Left))
            {
                var mousePto = new Ponto4D(MousePosition.X, MousePosition.Y);
                var pto = Utilitario.NDC_TelaSRU(Size.X, Size.Y, mousePto);
                //TODO: Corrigir para pegar as bordas da tela
                foreach(Objeto obj in mundo.ObjetosLista())
                {
                    var borda = obj.Bbox().Dentro(pto);

                    if (borda == true)
                    {
                        obj.Desenhar(new Transformacao4D());
                        objetoSelecionado = obj;
                    }
                    else
                        objetoSelecionado = null;
                }               
            }

            else if (MouseState.IsButtonPressed(MouseButton.Right)) // Dispara apenas uma vez
            {
                // Posição do mouse
                var mousePto = new Ponto4D(MousePosition.X, MousePosition.Y);
                // Ponto independente do tamanho da tela
                var sruPto = Utilitario.NDC_TelaSRU(Size.X, Size.Y, mousePto);

                if (objetoEmProgresso == null) // Um objeto está sendo criado/editado
                {
                    // Sempre precisa ter ao menos 2 pontos
                    var pontosPoligono = new List<Ponto4D>
                    {
                        sruPto,
                        sruPto
                    };

                    // Verificar se o ponto está sendo criado ou editado (selecionado)
                    if (objetoSelecionado != null) // Editando
                    {
                        objetoEmProgresso = new Poligono(objetoSelecionado, ref rotuloNovo, pontosPoligono);
                        objetoSelecionado = null;
                    }
                    else // Criando
                    {
                        objetoEmProgresso = new Poligono(mundo, ref rotuloNovo, pontosPoligono);
                    }
                }
                else // Construir um novo objeto
                {
                    objetoEmProgresso.PontosAdicionar(sruPto);
                }
            }

            // Dispara enquanto o botão estiver pressionado e o objeto estiver selecionado a cada frame
            if (MouseState.IsButtonDown(MouseButton.Button2) && objetoSelecionado != null)
            {
                Console.WriteLine("MouseState.IsButtonDown(MouseButton.Button2)");

                // Posição do mouse
                var mousePto = new Ponto4D(MousePosition.X, MousePosition.Y);
                // Ponto independente do tamanho da tela
                var sruPto = Utilitario.NDC_TelaSRU(Size.X, Size.Y, mousePto);

                // Atualizar o último ponto adicionado enquanto o botão está pressionado
                objetoSelecionado.PontosAlterar(sruPto, 0);
            }

            #endregion
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

            GL.DeleteBuffer(_vertexBufferObject_bbox);
            GL.DeleteVertexArray(_vertexArrayObject_bbox);

            GL.DeleteProgram(_shaderBranca.Handle);
            GL.DeleteProgram(_shaderVermelha.Handle);
            GL.DeleteProgram(_shaderVerde.Handle);
            GL.DeleteProgram(_shaderAzul.Handle);
            GL.DeleteProgram(_shaderCiano.Handle);
            GL.DeleteProgram(_shaderMagenta.Handle);
            GL.DeleteProgram(_shaderAmarela.Handle);

            base.OnUnload();
        }

        private void Gizmo_Sru3D()
        {
#if CG_Gizmo
#if CG_OpenGL
            var transform = Matrix4.Identity;
            GL.BindVertexArray(_vertexArrayObject_sruEixos);
            // EixoX
            _shaderVermelha.SetMatrix4("transform", transform);
            _shaderVermelha.Use();
            GL.DrawArrays(PrimitiveType.Lines, 0, 2);
            // EixoY
            _shaderVerde.SetMatrix4("transform", transform);
            _shaderVerde.Use();
            GL.DrawArrays(PrimitiveType.Lines, 2, 2);
            // EixoZ
            _shaderAzul.SetMatrix4("transform", transform);
            _shaderAzul.Use();
            GL.DrawArrays(PrimitiveType.Lines, 4, 2);
#endif
#endif
        }

#if CG_Gizmo
        private void Gizmo_BBox() 
        {
            if (objetoSelecionado != null)
            {
#if CG_OpenGL && !CG_DirectX

                float[] _bbox =
                {
                    (float)objetoSelecionado.Bbox().ObterMenorX, (float)objetoSelecionado.Bbox().ObterMenorY, 0.0f, // A
                    (float)objetoSelecionado.Bbox().ObterMaiorX, (float)objetoSelecionado.Bbox().ObterMenorY, 0.0f, // B
                    (float)objetoSelecionado.Bbox().ObterMaiorX, (float)objetoSelecionado.Bbox().ObterMaiorY, 0.0f, // C
                    (float)objetoSelecionado.Bbox().ObterMenorX, (float)objetoSelecionado.Bbox().ObterMaiorY, 0.0f // D
                };

                _vertexBufferObject_bbox = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject_bbox);
                GL.BufferData(BufferTarget.ArrayBuffer, _bbox.Length * sizeof(float), _bbox,
                    BufferUsageHint.StaticDraw);
                _vertexArrayObject_bbox = GL.GenVertexArray();
                GL.BindVertexArray(_vertexArrayObject_bbox);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
                GL.EnableVertexAttribArray(0);

                var transform = Matrix4.Identity;
                GL.BindVertexArray(_vertexArrayObject_bbox);
                _shaderAmarela.SetMatrix4("transform", transform);
                _shaderAmarela.Use();
                GL.DrawArrays(PrimitiveType.LineLoop, 0, (_bbox.Length / 3));
#endif
            }
        }
#endif
    }
}