using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VecUtils
{
    /**
     * <summary>Contains functions and constants used in multiple classes.
     * </summary>
     */
    public struct VecMath
    {

        /**
         * <summary>Computes the determinant of a two-dimensional square matrix
         * with rows consisting of the specified two-dimensional vectors.
         * </summary>
         *
         * <returns>The determinant of the two-dimensional square matrix.
         * </returns>
         *
         * <param name="vector1">The top row of the two-dimensional square
         * matrix.</param>
         * <param name="vector2">The bottom row of the two-dimensional square
         * matrix.</param>
         */
        internal static float Det(Vector2 vector1, Vector2 vector2)
        {
            return vector1.x * vector2.y - vector1.y * vector2.x;
        }

    }
}
