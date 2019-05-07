﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpGL;
using System.Drawing;

namespace PBR.IBLSpecularTextured {
    partial class CubemapNode : ModernNode, IRenderable {
        public static CubemapNode Create(Texture environmentMap, Texture texHDR) {
            var model = new CubeModel();
            var vs = new VertexShader(vertexCode);
            var fs = new FragmentShader(fragmentCode);
            var array = new ShaderArray(vs, fs);
            var map = new AttributeMap();
            map.Add("aPos", CubeModel.strPosition);
            var builder = new RenderMethodBuilder(array, map);
            var node = new CubemapNode(model, builder);
            node.ModelSize = new vec3(2, 2, 2);
            node.environmentMap = environmentMap;
            node.texHDR = texHDR;
            node.Initialize();

            return node;
        }

        private CubemapNode(IBufferSource model, params RenderMethodBuilder[] builders)
            : base(model, builders) {
        }

        // pbr: set up projection and view matrices for capturing data onto the 6 cubemap face directions
        mat4 captureProjection = glm.perspective((float)(Math.PI / 2.0), 1.0f, 0.1f, 10.0f);
        mat4[] captureViews =
		{
			glm.lookAt(new vec3(0.0f, 0.0f, 0.0f), new vec3( 1.0f,  0.0f,  0.0f), new vec3(0.0f, -1.0f,  0.0f)),
			glm.lookAt(new vec3(0.0f, 0.0f, 0.0f), new vec3(-1.0f,  0.0f,  0.0f), new vec3(0.0f, -1.0f,  0.0f)),
			glm.lookAt(new vec3(0.0f, 0.0f, 0.0f), new vec3( 0.0f,  1.0f,  0.0f), new vec3(0.0f,  0.0f,  1.0f)),
			glm.lookAt(new vec3(0.0f, 0.0f, 0.0f), new vec3( 0.0f, -1.0f,  0.0f), new vec3(0.0f,  0.0f, -1.0f)),
			glm.lookAt(new vec3(0.0f, 0.0f, 0.0f), new vec3( 0.0f,  0.0f,  1.0f), new vec3(0.0f, -1.0f,  0.0f)),
			glm.lookAt(new vec3(0.0f, 0.0f, 0.0f), new vec3( 0.0f,  0.0f, -1.0f), new vec3(0.0f, -1.0f,  0.0f))
		};
        protected unsafe override void DoInitialize() {
            base.DoInitialize();
            ViewportSwitch viewportSwitch = new ViewportSwitch(0, 0, 512, 512);
            // pbr: setup framebuffer
            var captureFBO = new Framebuffer(512, 512);
            captureFBO.Bind();
            var captureRBO = new Renderbuffer(512, 512, GL.GL_DEPTH_COMPONENT24);
            captureFBO.Attach(FramebufferTarget.Framebuffer, captureRBO, AttachmentLocation.Depth);
            captureFBO.CheckCompleteness();
            captureFBO.Unbind();

            // pbr: convert HDR equirectangular environment map to cubemap equivalent
            RenderMethod method = this.RenderUnit.Methods[0];
            ShaderProgram program = method.Program;
            program.SetUniform("equirectangularMap", this.texHDR);
            program.SetUniform("projection", captureProjection);
            viewportSwitch.On();
            for (uint i = 0; i < 6; ++i) {
                program.SetUniform("view", captureViews[i]);
                CubemapFace face = (CubemapFace)(GL.GL_TEXTURE_CUBE_MAP_POSITIVE_X + i);
                uint location = 0;
                int level = 0;
                captureFBO.Bind();
                captureFBO.Attach(FramebufferTarget.Framebuffer, location, face, this.environmentMap, level);
                captureFBO.CheckCompleteness();
                captureFBO.Unbind();

                captureFBO.Bind();
                GL.Instance.Clear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT);
                method.Render();
                captureFBO.Unbind();

                this.environmentMap.GetImage(face, 512, 512).Save(string.Format("texEnvCubemap.{0}.mip{1}.png", face, 0));
            }
            viewportSwitch.Off();
            captureFBO.Dispose();

            this.environmentMap.Bind();
            glGenerateMipmap(GL.GL_TEXTURE_CUBE_MAP);
            this.environmentMap.Unbind();
        }
        private static readonly GLDelegates.void_uint glGenerateMipmap;

        static CubemapNode() {
            glGenerateMipmap = GL.Instance.GetDelegateFor("glGenerateMipmap", GLDelegates.typeof_void_uint) as GLDelegates.void_uint;
        }

        private Texture environmentMap;
        private Texture texHDR;

        private ThreeFlags enableRendering = ThreeFlags.None;//  ThreeFlags.BeforeChildren | ThreeFlags.Children | ThreeFlags.AfterChildren;
        /// <summary>
        /// Render before/after children? Render children? 
        /// RenderAction cares about this property. Other actions, maybe, maybe not, your choice.
        /// </summary>
        public ThreeFlags EnableRendering {
            get { return this.enableRendering; }
            set { this.enableRendering = value; }
        }

        public void RenderBeforeChildren(RenderEventArgs arg) {
            ICamera camera = arg.Camera;
            mat4 projection = camera.GetProjectionMatrix();
            mat4 view = camera.GetViewMatrix();
            mat4 model = this.GetModelMatrix();
            mat4 normal = glm.transpose(glm.inverse(view * model));

            RenderMethod method = this.RenderUnit.Methods[0];
            ShaderProgram program = method.Program;
            program.SetUniform("projection", projection);
            program.SetUniform("view", view * model);
            program.SetUniform("equirectangularMap", this.texHDR);

            method.Render();
        }

        public void RenderAfterChildren(RenderEventArgs arg) {
            // nothing to do.
        }
    }
}
