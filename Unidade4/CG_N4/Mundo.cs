#define CG_Gizmo // debugar gráfico.
#define CG_OpenGL // render OpenGL.

using CG_Biblioteca;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using System;
using OpenTK.Mathematics;
using System.Reflection.Metadata;
using System.Resources;

namespace gcgcg
{
    public class Mundo : GameWindow
    {
        Objeto mundo;
        private char rotuloNovo = '?';

        private readonly float[] _sruEixos =
        {
            -0.5f, 0.0f, 0.0f, /* X- */ 0.5f, 0.0f, 0.0f, /* X+ */
            0.0f, -0.5f, 0.0f, /* Y- */ 0.0f, 0.5f, 0.0f, /* Y+ */
            0.0f, 0.0f, -0.5f, /* Z- */ 0.0f, 0.0f, 0.5f /* Z+ */
                  };

        private int _vertexBufferObject;
        private int _vaoModel;

        private Shader _shaderBranca;
        private Shader _shaderVermelha;
        private Shader _shaderVerde;
        private Shader _shaderAzul;
        private Shader _shaderCiano;
        private Shader _shaderMagenta;
        private Shader _shaderAmarela;
        
        private Texture _texture;
        private Texture _texture1;
        private Texture _diffuseMap;

        private Texture _specularMap;
        private Shader _lightingMapShader;
        private Shader basicLightingShader;
        private IluminacaoAtual _estadoIluminacao = IluminacaoAtual.Nenhuma;

        // Camera
        private Camera _camera;
        private bool _firstMove = true;
        private Vector2 _lastPos;
        private Vector3 _origin = new(0, 0, 0); // Origem do espaço 3D

        private Cubo _centralCube;
        
        // Orbiting cube
        private Cubo _orbitingCube;
        private float orbitSpeed = 0.15f; // Speed of the orbit
        private float angle = 0.0f; // Initial angle

        public Mundo(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            mundo = new Objeto(null, ref rotuloNovo);
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            #region Cores

            _shaderBranca = new Shader("Shaders/shader.vert", "Shaders/shaderBranca.frag");
            _shaderVermelha = new Shader("Shaders/shader.vert", "Shaders/shaderVermelha.frag");
            _shaderVerde = new Shader("Shaders/shader.vert", "Shaders/shaderVerde.frag");
            _shaderAzul = new Shader("Shaders/shader.vert", "Shaders/shaderAzul.frag");
            _shaderCiano = new Shader("Shaders/shader.vert", "Shaders/shaderCiano.frag");
            _shaderMagenta = new Shader("Shaders/shader.vert", "Shaders/shaderMagenta.frag");
            _shaderAmarela = new Shader("Shaders/shader.vert", "Shaders/shaderAmarela.frag");

            #endregion

            #region Eixos: SRU

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _sruEixos.Length * sizeof(float), _sruEixos,
                BufferUsageHint.StaticDraw);
            _vaoModel = GL.GenVertexArray();
            GL.BindVertexArray(_vaoModel);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);


            _lightingMapShader = new Shader("Shaders/lightingMap.vert", "Shaders/lightingMap.frag");
            basicLightingShader = new Shader("Shaders/basicLighting.vert", "Shaders/lighting.frag");




            _texture = Texture.LoadFromFile("Resources/adam.jpg");
            _texture.Use(TextureUnit.Texture0);
            _texture1 = Texture.LoadFromFile("Resources/ricardo.jpg");
            _texture1.Use(TextureUnit.Texture1);

            GL.GetError();

            #endregion

            #region Objeto: Cubo maior
            _centralCube = new Cubo(mundo, ref rotuloNovo);
            _centralCube.shaderCor = _shaderCiano;
            #endregion

            #region Objeto: Cubo menor
            _orbitingCube = new Cubo(mundo, ref rotuloNovo);
            _orbitingCube.MatrizEscalaXYZ(0.3, 0.3, 0.3);
            _orbitingCube.MatrizTranslacaoXYZ(2.7, 0.8, 0);
            _orbitingCube.shaderCor = _shaderAmarela;
            #endregion

            _diffuseMap = Texture.LoadFromFile("Resources/container2.png");
            _specularMap = Texture.LoadFromFile("Resources/container2_specular.png");

            _camera = new Camera(Vector3.UnitZ * 5, Size.X / (float)Size.Y);
            CursorState = CursorState.Grabbed;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _texture.Use(TextureUnit.Texture0);
            _texture1.Use(TextureUnit.Texture1);

            switch (_estadoIluminacao)
            {
                case IluminacaoAtual.BasicLighting:
                    // Configuração do shader para BasicLighting
                    basicLightingShader.Use();
                    basicLightingShader.SetVector3("objectColor", new Vector3(0.0f, 1.0f, 1.0f)); // Cor do objeto
                    basicLightingShader.SetVector3("lightColor", new Vector3(1.0f, 1.0f, 1.0f));  // Luz branca
                    basicLightingShader.SetVector3("lightPos", new Vector3(1.2f, 1.0f, 2.0f));    // Posição da luz

                    // Atualize as matrizes view e projection
                    basicLightingShader.SetMatrix4("view", _camera.GetViewMatrix());
                    basicLightingShader.SetMatrix4("projection", _camera.GetProjectionMatrix());
                    basicLightingShader.SetMatrix4("model", Matrix4.Identity);

                    // Atualize a posição da câmera
                    basicLightingShader.SetVector3("viewPos", _camera.Position);

                    // Renderize o objeto central com o shader de iluminação
                    _centralCube.shaderCor = basicLightingShader;

                    // Renderizar o cubo central
                    _centralCube.Desenhar(new Transformacao4D(), _camera);

                    // Renderizar o cubo orbitante com o mesmo shader
                    _orbitingCube.Desenhar(new Transformacao4D(), _camera);
                    break;
                case IluminacaoAtual.LightingMaps:
                    // Vincular as texturas
                    _diffuseMap.Use(TextureUnit.Texture0);
                    _specularMap.Use(TextureUnit.Texture1);
                    _lightingMapShader.Use();

                    _lightingMapShader.SetMatrix4("model", Matrix4.Identity);
                    _lightingMapShader.SetMatrix4("view", _camera.GetViewMatrix());
                    _lightingMapShader.SetMatrix4("projection", _camera.GetProjectionMatrix());
                    _lightingMapShader.SetVector3("viewPos", _camera.Position);

                    // Configurações dos materiais
                    _lightingMapShader.SetInt("material.diffuse", 0);
                    _lightingMapShader.SetInt("material.specular", 1);
                    _lightingMapShader.SetVector3("material.specular", new Vector3(0.5f, 0.5f, 0.5f));
                    _lightingMapShader.SetFloat("material.shininess", 32.0f);

                    // Configurações de iluminação
                    _lightingMapShader.SetVector3("light.position", new Vector3(1.2f, 1.0f, 2.0f));
                    _lightingMapShader.SetVector3("light.ambient", new Vector3(0.2f));
                    _lightingMapShader.SetVector3("light.diffuse", new Vector3(0.5f));
                    _lightingMapShader.SetVector3("light.specular", new Vector3(1.0f));

                    _centralCube.shaderCor = _lightingMapShader;
                    // Renderizar o cubo central
                    _centralCube.Desenhar(new Transformacao4D(), _camera);

                    break;
                case IluminacaoAtual.Nenhuma:
                    _centralCube.shaderCor = _shaderCiano;
                    _centralCube.Desenhar(new Transformacao4D(), _camera);
                    break;
                default:
                    // Renderização padrão
                    break;
            }

            
            
            mundo.Desenhar(new Transformacao4D(), _camera);

            GL.GetError();

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (!IsFocused)
            {
                return;
            }
            
            _orbitingCube.MatrizRotacao(orbitSpeed);

            #region Teclado

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
                Close();

            const float cameraSpeed = 3.5f;
            const float sensitivity = 1f;

            /*
            if (input.IsKeyPressed(Keys.M) && objetoSelecionado != null)
                objetoSelecionado.MatrizImprimir();
            if (input.IsKeyPressed(Keys.I) && objetoSelecionado != null)
                objetoSelecionado.MatrizAtribuirIdentidade();
            if (input.IsKeyPressed(Keys.Left) && objetoSelecionado != null)
                objetoSelecionado.MatrizTranslacaoXYZ(-0.05, 0, 0);
            if (input.IsKeyPressed(Keys.Right) && objetoSelecionado != null)
                objetoSelecionado.MatrizTranslacaoXYZ(0.05, 0, 0);
            if (input.IsKeyPressed(Keys.Up) && objetoSelecionado != null)
                objetoSelecionado.MatrizTranslacaoXYZ(0, 0.05, 0);
            if (input.IsKeyPressed(Keys.Down) && objetoSelecionado != null)
                objetoSelecionado.MatrizTranslacaoXYZ(0, -0.05, 0);
            if (input.IsKeyPressed(Keys.O) && objetoSelecionado != null)
                objetoSelecionado.MatrizTranslacaoXYZ(0, 0, 0.05);
            if (input.IsKeyPressed(Keys.L) && objetoSelecionado != null)
                objetoSelecionado.MatrizTranslacaoXYZ(0, 0, -0.05);
            if (input.IsKeyPressed(Keys.PageUp) && objetoSelecionado != null)
                objetoSelecionado.MatrizEscalaXYZ(2, 2, 2);
            if (input.IsKeyPressed(Keys.PageDown) && objetoSelecionado != null)
                objetoSelecionado.MatrizEscalaXYZ(0.5, 0.5, 0.5);
            if (input.IsKeyPressed(Keys.Home) && objetoSelecionado != null)
                objetoSelecionado.MatrizEscalaXYZBBox(0.5, 0.5, 0.5);
            if (input.IsKeyPressed(Keys.End) && objetoSelecionado != null)
                objetoSelecionado.MatrizEscalaXYZBBox(2, 2, 2);
            if (input.IsKeyPressed(Keys.D1) && objetoSelecionado != null)
                objetoSelecionado.MatrizRotacao(10);
            if (input.IsKeyPressed(Keys.D2) && objetoSelecionado != null)
                objetoSelecionado.MatrizRotacao(-10);
            if (input.IsKeyPressed(Keys.D3) && objetoSelecionado != null)
                objetoSelecionado.MatrizRotacaoZBBox(10);
            if (input.IsKeyPressed(Keys.D4) && objetoSelecionado != null)
                objetoSelecionado.MatrizRotacaoZBBox(-10);*/
            if (input.IsKeyPressed(Keys.D0))
                _estadoIluminacao = IluminacaoAtual.Nenhuma;
            if (input.IsKeyPressed(Keys.D1))
                _estadoIluminacao = IluminacaoAtual.BasicLighting;
            if (input.IsKeyPressed(Keys.D2))
                _estadoIluminacao = IluminacaoAtual.LightingMaps;

            if (input.IsKeyDown(Keys.Z))
            {
                _camera.Position = Vector3.UnitZ * 5;
            }

            // Calcula os vetores de direção da câmera (front, right, up) com base na posição da câmera
            
            // _origin - _camera.Position: Retorna um vetor apontando da posição da câmera em direção a origem.
            var front = Vector3.Normalize(_origin - _camera.Position);
            
            // Vector3.Cross(front, Vector3.UnitY): Calcula o produto vetorial entre os vetores front e up (UnitY)
            var right = Vector3.Normalize(Vector3.Cross(front, Vector3.UnitY));
            
            // Vector3.Cross(front, Vector3.UnitY): Calcula o produto vetorial entre os vetores right e front
            var up = Vector3.Normalize(Vector3.Cross(right, front));
            
            // O produto vetorial de dois vetores resulta em um vetor perpendicular a ambos, fornecendo a direção
            // da direita relativa à direção voltada para a câmera.
            // Vector3.Normalize(...): A normalização dos vetores garante sejam um vetor unitário.

            if (input.IsKeyDown(Keys.W))
            {
                _camera.Position += front * cameraSpeed * (float)e.Time; // Forward
            }

            if (input.IsKeyDown(Keys.S))
            {
                _camera.Position -= front * cameraSpeed * (float)e.Time; // Backwards
            }

            if (input.IsKeyDown(Keys.A))
            {
                _camera.Position -= right * cameraSpeed * (float)e.Time; // Left
            }

            if (input.IsKeyDown(Keys.D))
            {
                _camera.Position += right * cameraSpeed * (float)e.Time; // Right
            }

            if (input.IsKeyDown(Keys.Space))
            {
                _camera.Position += up * cameraSpeed * (float)e.Time; // Up
            }

            if (input.IsKeyDown(Keys.LeftShift))
            {
                _camera.Position -= up * cameraSpeed * (float)e.Time; // Down
            }

            #endregion

            #region Mouse

            var mouse = MouseState;

            if (_firstMove)
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }
            else
            {
                var deltaX = mouse.X - _lastPos.X;
                var deltaY = mouse.Y - _lastPos.Y;
                _lastPos = new Vector2(mouse.X, mouse.Y);

                // Atualizar o yaw baseado no movimento e sensibilidade
                _camera.Yaw += deltaX * sensitivity;

                // Calcular o movimento do vetor baseado nos vetores right, front e up (Movimento A/D)
                var movement = (right * deltaX + front * deltaY) * sensitivity * cameraSpeed * (float)e.Time;

                // Atualizar a posição da câmera
                _camera.Position += movement;

                // Adicionar movimento vertical baseado no deltaY e vetor up (movimento Shift/Space)
                _camera.Position += up * deltaY * sensitivity * cameraSpeed * (float)e.Time;
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

            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteVertexArray(_vaoModel);

            GL.DeleteProgram(_shaderBranca.Handle);
            GL.DeleteProgram(_shaderVermelha.Handle);
            GL.DeleteProgram(_shaderVerde.Handle);
            GL.DeleteProgram(_shaderAzul.Handle);
            GL.DeleteProgram(_shaderCiano.Handle);
            GL.DeleteProgram(_shaderMagenta.Handle);
            GL.DeleteProgram(_shaderAmarela.Handle);

            base.OnUnload();
        }
    }
    public enum IluminacaoAtual
    {
        Nenhuma,
        BasicLighting,
        LightingMaps
    }
}