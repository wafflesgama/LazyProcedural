﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Spatial;
using Sceelix.Meshes.Data;
using Sceelix.Surfaces.Data;
using Sceelix.Surfaces.Procedures;

namespace Sceelix.Paths.Parameters
{
    /// <summary>
    /// Paints mesh faces on the surface.
    /// </summary>
    /// <seealso cref="SurfacePaintProcedure.SurfacePaintingParameter" />
    public class MeshOnSurfacePainting : SurfacePaintProcedure.SurfacePaintingParameter
    {
        /// <summary>
        /// The meshes that are to be painted on the terrain.
        /// </summary>
        private readonly CollectiveInput<MeshEntity> _inputMesh = new CollectiveInput<MeshEntity>("Mesh");

        /// <summary>
        /// The meshes that were painted on the terrain.
        /// </summary>
        private readonly Output<MeshEntity> _outputMesh = new Output<MeshEntity>("Mesh");

        /// <summary>
        /// The intensity of the texture painting.
        /// </summary>
        private readonly FloatParameter _parameterValue = new FloatParameter("Value", 0.5f)
        {
            MinValue = 0,
            MaxValue = 1,
            Increment = 0.01f
        };



        public MeshOnSurfacePainting()
            : base("Mesh")
        {
        }



        public bool ContainsPoint(List<UnityEngine.Vector2> vertices, UnityEngine.Vector2 point)
        {
            bool conclusion = false;

            for (int i = 0, j = vertices.Count - 1; i < vertices.Count; j = i++)
                if (vertices[i].y > point.y != vertices[j].y > point.y &&
                    point.x < (vertices[j].x - vertices[i].x) * (point.y - vertices[i].y) / (vertices[j].y - vertices[i].y) + vertices[i].x)
                    conclusion = !conclusion;

            return conclusion;
        }



        public override Action<SurfaceEntity, float[][,]> GetApplyFunction()
        {
            var textureIndex = ParameterTextureIndex.Value;
            var value = _parameterValue.Value;

            var meshEntities = _inputMesh.Read().ToList();

            _outputMesh.Write(meshEntities);

            return (surfaceEntity, values) =>
            {
                var surfaceEntityBoundingRectangle = surfaceEntity.BoundingRectangle;

                foreach (MeshEntity meshEntity in meshEntities)
                foreach (var face in meshEntity.Faces)
                {
                    var points = face.Vertices.Select(x => x.Position.ToVector2()).ToList();


                    BoundingRectangle faceBoundingRectangle = face.BoundingRectangle;

                    if (!faceBoundingRectangle.Intersects(surfaceEntityBoundingRectangle))
                        continue;

                    /*if(!surfaceEntity.Contains(faceBoundingRectangle.Min) && !surfaceEntity.Contains(faceBoundingRectangle.Max))
                            continue;*/

                    var minCoords = surfaceEntity.ToCoordinates(UnityEngine.Vector2.Maximize(faceBoundingRectangle.Min, surfaceEntityBoundingRectangle.Min));
                    var maxCoords = surfaceEntity.ToCoordinates(UnityEngine.Vector2.Minimize(faceBoundingRectangle.Max, surfaceEntityBoundingRectangle.Max));


                    for (int i = minCoords.x; i <= maxCoords.x; i++)
                    for (int j = maxCoords.y; j <= minCoords.y; j++)
                    {
                        //var height surfaceEntity.Heights[i, j];
                        UnityEngine.Vector2 gridCornerPosition = surfaceEntity.ToWorldPosition(new Coordinate(i, j));

                        if (ContainsPoint(points, gridCornerPosition)) SetDataIndex(i, j, values, textureIndex, value);
                    }
                }
            };
        }
    }
}