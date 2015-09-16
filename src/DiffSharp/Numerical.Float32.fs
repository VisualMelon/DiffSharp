﻿//
// This file is part of
// DiffSharp: Automatic Differentiation Library
//
// Copyright (c) 2014--2015, National University of Ireland Maynooth (Atilim Gunes Baydin, Barak A. Pearlmutter)
// 
// Released under the LGPL license.
//
//   DiffSharp is free software: you can redistribute it and/or modify
//   it under the terms of the GNU Lesser General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   DiffSharp is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General Public License for more details.
//
//   You should have received a copy of the GNU Lesser General Public License
//   along with DiffSharp. If not, see <http://www.gnu.org/licenses/>.
//
// Written by:
//
//   Atilim Gunes Baydin
//   atilimgunes.baydin@nuim.ie
//
//   Barak A. Pearlmutter
//   barak@cs.nuim.ie
//
//   Brain and Computation Lab
//   Hamilton Institute & Department of Computer Science
//   National University of Ireland Maynooth
//   Maynooth, Co. Kildare
//   Ireland
//
//   www.bcl.hamilton.ie
//


/// Numerical differentiation module
module DiffSharp.Numerical.Float32

open DiffSharp.Util
open DiffSharp.Config

/// Numerical differentiation operations module (automatically opened)
[<AutoOpen>]
module DiffOps =
    /// First derivative of a scalar-to-scalar function `f`, at point `x`
    let inline diff (f:float32->float32) x =
        ((f (x + GlobalConfig.Float32Epsilon)) - (f (x - GlobalConfig.Float32Epsilon))) * GlobalConfig.Float32EpsilonRec2
    
    /// Original value and first derivative of a scalar-to-scalar function `f`, at point `x`
    let inline diff' f x =
        (f x, diff f x)

    /// Gradient-vector product (directional derivative) of a vector-to-scalar function `f`, at point `x`, along vector `v`
    let inline gradv (f:float32[]->float32) (x:float32[]) (v:float32[]) =
        let veps = GlobalConfig.Float32BackEnd.Mul_S_V(GlobalConfig.Float32Epsilon, v)
        ((f (GlobalConfig.Float32BackEnd.Add_V_V(x, veps))) - (f (GlobalConfig.Float32BackEnd.Sub_V_V(x, veps)))) * GlobalConfig.Float32EpsilonRec2

    /// Original value and gradient-vector product (directional derivative) of a vector-to-scalar function `f`, at point `x`, along vector `v`
    let inline gradv' f x v =
        (f x, gradv f x v)

    /// Second derivative of a scalar-to-scalar function `f`, at point `x`
    let inline diff2 f x =
        ((f (x + GlobalConfig.Float32Epsilon)) - 2.f * (f x) + (f (x - GlobalConfig.Float32Epsilon))) / (GlobalConfig.Float32Epsilon * GlobalConfig.Float32Epsilon)

    /// Original value and second derivative of a scalar-to-scalar function `f`, at point `x`
    let inline diff2' f x =
        (f x, diff2 f x)

    /// Original value, first derivative, and second derivative of a scalar-to-scalar function `f`, at point `x`
    let inline diff2'' f x =
        (f x, diff f x, diff2 f x)

    /// Original value and gradient of a vector-to-scalar function `f`, at point `x`
    let inline grad' (f:float32[]->float32) x =
        let fx = f x
        let g = Array.create x.Length fx
        let gg = Array.init x.Length (fun i -> f (GlobalConfig.Float32BackEnd.Add_V_V(x, standardBasisVal x.Length i GlobalConfig.Float32Epsilon)))
        (fx, GlobalConfig.Float32BackEnd.Mul_S_V(GlobalConfig.Float32EpsilonRec, GlobalConfig.Float32BackEnd.Sub_V_V(gg, g)))
    
    /// Gradient of a vector-to-scalar function `f`, at point `x`
    let grad f x =
        grad' f x |> snd

    /// Original value, gradient, and Hessian of a vector-to-scalar function `f`, at point `x`
    let inline gradhessian' f x =
        let (fx, g) = grad' f x
        let h = array2D (Array.create x.Length g)
        let hh = array2D (Array.init x.Length (fun i -> grad f (GlobalConfig.Float32BackEnd.Add_V_V(x, standardBasisVal x.Length i GlobalConfig.Float32Epsilon))))
        (fx, g, GlobalConfig.Float32BackEnd.Mul_S_M(GlobalConfig.Float32EpsilonRec, GlobalConfig.Float32BackEnd.Sub_M_M(hh, h)))

    /// Gradient and Hessian of a vector-to-scalar function `f`, at point `x`
    let inline gradhessian f x =
        gradhessian' f x |> sndtrd

    /// Original value and Hessian of a vector-to-scalar function `f`, at point `x`
    let inline hessian' f x =
        gradhessian' f x |> fsttrd
                
    /// Hessian of a vector-to-scalar function `f`, at point `x`
    let inline hessian f x =
        gradhessian' f x |> trd

    /// Original value and Hessian-vector product of a vector-to-scalar function `f`, at point `x`, along vector `v`
    let inline hessianv' (f:float32[]->float32) (x:float32[]) (v:float32[]) =
        let veps = GlobalConfig.Float32BackEnd.Mul_S_V(GlobalConfig.Float32Epsilon, v)
        let fx, gg1 = grad' f (GlobalConfig.Float32BackEnd.Add_V_V(x, veps))
        let gg2 = grad f (GlobalConfig.Float32BackEnd.Sub_V_V(x, veps))
        (fx, GlobalConfig.Float32BackEnd.Mul_S_V(GlobalConfig.Float32EpsilonRec2, GlobalConfig.Float32BackEnd.Sub_V_V(gg1, gg2)))

    /// Hessian-vector product of a vector-to-scalar function `f`, at point `x`, along vector `v`
    let inline hessianv (f:float32[]->float32) x v =
        hessianv' f x v |> snd

    /// Original value, gradient-vector product (directional derivative), and Hessian-vector product of a vector-to-scalar funtion `f`, at point `x`, along vector `v`
    let inline gradhessianv' (f:float32[]->float32) x v =
        let fx, gv = gradv' f x v
        let hv = hessianv f x v
        (fx, gv, hv)

    /// Gradient-vector product (directional derivative) and Hessian-vector product of a vector-to-scalar function `f`, at point `x`, along vector `v`
    let inline gradhessianv (f:float32[]->float32) x v =
        gradhessianv' f x v |> sndtrd

    /// Original value and Laplacian of a vector-to-scalar function `f`, at point `x`
    let inline laplacian' f x =
        let (v, h) = hessian' f x in (v, GlobalConfig.Float32BackEnd.Sum_V(GlobalConfig.Float32BackEnd.Diagonal_M(h)))

    /// Laplacian of a vector-to-scalar function `f`, at point `x`
    let inline laplacian f x =
        laplacian' f x |> snd

    /// Original value and transposed Jacobian of a vector-to-vector function `f`, at point `x`
    let inline jacobianT' (f:float32[]->float32[]) x =
        let fx = f x
        let j = array2D (Array.create x.Length fx)
        let jj = array2D (Array.init x.Length (fun i -> f (GlobalConfig.Float32BackEnd.Add_V_V(x, standardBasisVal x.Length i GlobalConfig.Float32Epsilon))))
        (fx, GlobalConfig.Float32BackEnd.Mul_S_M(GlobalConfig.Float32EpsilonRec, GlobalConfig.Float32BackEnd.Sub_M_M(jj, j)))

    /// Transposed Jacobian of a vector-to-vector function `f`, at point `x`
    let inline jacobianT f x =
        jacobianT' f x |> snd

    /// Original value and Jacobian of a vector-to-vector function `f`, at point `x`
    let inline jacobian' f x =
        jacobianT' f x |> fun (r, j) -> (r, GlobalConfig.Float32BackEnd.Transpose_M(j))

    /// Jacobian of a vector-to-vector function `f`, at point `x`
    let inline jacobian f x =
        jacobian' f x |> snd

    /// Jacobian-vector product of a vector-to-vector function `f`, at point `x`, along vector `v`
    let inline jacobianv (f:float32[]->float32[]) x v =
        let veps = GlobalConfig.Float32BackEnd.Mul_S_V(GlobalConfig.Float32Epsilon, v)
        GlobalConfig.Float32BackEnd.Mul_S_V(GlobalConfig.Float32EpsilonRec2, GlobalConfig.Float32BackEnd.Sub_V_V(f (GlobalConfig.Float32BackEnd.Add_V_V(x, veps)), f (GlobalConfig.Float32BackEnd.Sub_V_V(x, veps))))

    /// Original value and Jacobian-vector product of a vector-to-vector function `f`, at point `x`, along vector `v`
    let inline jacobianv' f x v =
        (f x, jacobianv f x v)

    /// Original value and curl of a vector-to-vector function `f`, at point `x`. Supported only for functions with a three-by-three Jacobian matrix.
    let inline curl' f x =
        let v, j = jacobianT' f x
        if (Array2D.length1 j, Array2D.length2 j) <> (3, 3) then ErrorMessages.InvalidArgCurl()
        v, [|j.[1, 2] - j.[2, 1]; j.[2, 0] - j.[0, 2]; j.[0, 1] - j.[1, 0]|]

    /// Curl of a vector-to-vector function `f`, at point `x`. Supported only for functions with a three-by-three Jacobian matrix.
    let inline curl f x =
        curl' f x |> snd

    /// Original value and divergence of a vector-to-vector function `f`, at point `x`. Defined only for functions with a square Jacobian matrix.
    let inline div' f x =
        let v, j = jacobianT' f x
        if Array2D.length1 j <> Array2D.length2 j then ErrorMessages.InvalidArgDiv()
        v, GlobalConfig.Float32BackEnd.Sum_V(GlobalConfig.Float32BackEnd.Diagonal_M(j))

    /// Divergence of a vector-to-vector function `f`, at point `x`. Defined only for functions with a square Jacobian matrix.
    let inline div f x =
        div' f x |> snd

    /// Original value, curl, and divergence of a vector-to-vector function `f`, at point `x`. Supported only for functions with a three-by-three Jacobian matrix.
    let inline curldiv' f x =
        let v, j = jacobianT' f x
        if (Array2D.length1 j, Array2D.length2 j) <> (3, 3) then ErrorMessages.InvalidArgCurlDiv()
        v, [|j.[1, 2] - j.[2, 1]; j.[2, 0] - j.[0, 2]; j.[0, 1] - j.[1, 0]|], j.[0, 0] + j.[1, 1] + j.[2, 2]

    /// Curl and divergence of a vector-to-vector function `f`, at point `x`. Supported only for functions with a three-by-three Jacobian matrix.
    let inline curldiv f x =
        curldiv' f x |> sndtrd

