﻿using GLM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpGL.Objects.ColorCodedPicking
{
    /// <summary>
    /// The picked geometry of color-coded picking.
    /// <para>Representing a basic geometry.</para>
    /// </summary>
    public interface IPickedGeometry
    {
        /// <summary>
        /// Gets or sets primitive's geometry type.
        /// </summary>
        GeometryTypes GeometryType { get; set; }

        /// <summary>
        /// Gets or sets positions of this primitive's vertices.
        /// </summary>
        vec3[] positions { get; set; }

        /// <summary>
        /// The scene's element from which this <see cref="IPickedGeometry"/>'s instance is picked.
        /// </summary>
        IColorCodedPicking From { get; set; }

        /// <summary>
        /// The last vertex's id that constructs the picked primitive.
        /// <para>This id is in scene's all <see cref="IColorCodedPicking"/>s' order.</para>
        /// </summary>
        uint StageVertexID { get; set; }
    }
}
