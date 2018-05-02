﻿using System;
using System.ComponentModel;
using System.Drawing.Design;

namespace CSharpGL
{
    // 没有显式索引时的渲染方法。
    /// <summary>
    /// Wraps glDrawArrays(uint mode, int first, int count).
    /// </summary>
    [Editor(typeof(PropertyGridEditor), typeof(UITypeEditor))]
    public class DrawArraysCmd : IDrawCommand
    {
        /// <summary>
        /// Wraps glDrawArrays(uint mode, int first, int count).
        /// </summary>
        /// <param name="mode">用哪种方式渲染各个顶点？（GL.GL_TRIANGLES etc.）</param>
        /// <param name="firstVertex">要渲染的第一个顶点的位置。<para>Index of first vertex to be rendered.</para></param>
        /// <param name="vertexCount">要渲染多少个元素？<para>How many vertexes to be rendered?</para></param>
        /// <param name="frameCount">How many frames are there?</param>
        public DrawArraysCmd(DrawMode mode, int firstVertex, int vertexCount, int frameCount = 1)
        {
            this.Mode = mode;
            this.FirstVertex = firstVertex;
            this.VertexCount = vertexCount;
            this.RenderingVertexCount = vertexCount;
            this.FrameCount = frameCount;
        }

        /// <summary>
        /// 此VBO含有多少个元素？
        /// <para>How many elements in thie buffer?</para>
        /// </summary>
        public int VertexCount { get; private set; }

        /// <summary>
        /// How many frames are there?
        /// </summary>
        [Category("ControlMode.ByFrame")]
        public int FrameCount { get; set; }

        /// <summary>
        /// Gets or sets index of current frame.
        /// </summary>
        [Category("ControlMode.ByFrame")]
        public int CurrentFrame { get; set; }


        /// <summary>
        /// 要渲染的第一个顶点的位置。<para>Index of first vertex to be rendered.</para>
        /// </summary>
        [Category("ControlMode.Random")]
        public int FirstVertex { get; set; }

        /// <summary>
        /// 要渲染多少个元素？<para>How many vertexes to be rendered?</para>
        /// </summary>
        [Category("ControlMode.Random")]
        public int RenderingVertexCount { get; set; }

        #region IDrawCommand

        /// <summary>
        /// 用哪种方式渲染各个顶点？（GL.GL_TRIANGLES etc.）
        /// </summary>
        public DrawMode Mode { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="indexAccessMode">index buffer is accessable randomly or only by frame.</param>
        public void Draw(IndexAccessMode indexAccessMode)
        {
            uint mode = (uint)this.Mode;

            int frameCount = this.FrameCount;
            if (frameCount < 1) { throw new Exception("error: frameCount is less than 1."); }

            switch (indexAccessMode)
            {
                case IndexAccessMode.ByFrame:
                    int vertexCount = this.VertexCount;
                    if (frameCount == 1)
                    {
                        GL.Instance.DrawArrays(mode, 0, vertexCount);
                    }
                    else
                    {
                        int vertexCountPerFrame = vertexCount / frameCount;
                        GL.Instance.DrawArrays(mode, this.CurrentFrame * vertexCountPerFrame, vertexCountPerFrame);
                    }
                    break;
                case IndexAccessMode.Random:
                    GL.Instance.DrawArrays(mode, this.FirstVertex, this.RenderingVertexCount);
                    break;
                default:
                    throw new NotDealWithNewEnumItemException(typeof(IndexAccessMode));
            }

        }

        #endregion IDrawCommand

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            int frameCount = this.FrameCount;
            if (frameCount < 1) { throw new Exception("error: frameCount is less than 1."); }

            var builder = new System.Text.StringBuilder();

            var mode = this.Mode;
            int vertexCount = this.VertexCount;

            builder.AppendLine("ControlMode.ByFrame:");
            if (frameCount == 1)
            {
                builder.AppendLine(string.Format("glDrawArrays(mode: {0}, first: {1}, count: {2});", mode, 0, vertexCount));
            }
            else
            {
                int vertexCountPerFrame = vertexCount / frameCount;
                builder.AppendLine(string.Format("glDrawArrays(mode: {0}, first = currentFrame * vertexCountPerFrame: {1} = {2} * {3}, count = vertexCountPerFrame: {4});", mode, this.CurrentFrame * vertexCountPerFrame, this.CurrentFrame, vertexCountPerFrame, vertexCountPerFrame));
            }

            builder.AppendLine("ControlMode.Random:");
            builder.AppendLine(string.Format("glDrawArrays(mode: {0}, first: {1}, count: {2});", mode, this.FirstVertex, this.RenderingVertexCount));

            return builder.ToString();
        }
    }
}