namespace Tests

open NUnit.Framework
open DiffSharp

[<TestFixture>]
type TestDiffSharp () =

    let rosenbrock (x:Tensor) = (1. - x.[0])**2 + 100. * (x.[1] - x.[0]**2)**2
    let rosenbrockGrad (x:Tensor) = dsharp.stack([-2*(1-x.[0])-400*x.[0]*(-(x.[0]**2) + x.[1]); 200*(-(x.[0]**2) + x.[1])])

    let fvect2vect2 (x:Tensor) = 
        let x, y = x.[0], x.[1]
        dsharp.stack([x*x*y; 5*x+sin y])
    let fvect2vect2Jacobian (x:Tensor) = 
        let x, y = x.[0], x.[1]
        dsharp.tensor([[2*x*y; x*x];[dsharp.tensor(5.); cos y]])

    let fvect3vect2 (x:Tensor) = 
        let x, y, z = x.[0], x.[1], x.[2]
        dsharp.stack([x*y+2*y*z;2*x*y*y*z])
    let fvect3vect2Jacobian (x:Tensor) = 
        let x, y, z = x.[0], x.[1], x.[2]
        dsharp.tensor([[y;x+2*z;2*y];[2*y*y*z;4*x*y*z;2*x*y*y]])

    let fvect3vect4 (x:Tensor) =
        let y1, y2, y3, y4 = x.[0], 5*x.[2], 4*x.[1]*x.[1]-2*x.[2],x.[2]*sin x.[0]
        dsharp.stack([y1;y2;y3;y4])
    let fvect3vect4Jacobian (x:Tensor) =
        let z, o = dsharp.zero(), dsharp.one()
        dsharp.tensor([[o,z,z],[z,z,5*o],[z,8*x.[1],-2*o],[x.[2]*cos x.[0],z,sin x.[0]]])

    [<SetUp>]
    member this.Setup () =
        ()

    [<Test>]
    member this.TestZeros () =
        let t = dsharp.zeros([2;3])
        let tCorrect = dsharp.tensor([[0,0,0],[0,0,0]])
        Assert.AreEqual(tCorrect, t)

    [<Test>]
    member this.TestOnes () =
        let t = dsharp.ones([2;3])
        let tCorrect = dsharp.tensor([[1,1,1],[1,1,1]])
        Assert.AreEqual(tCorrect, t)

    [<Test>]
    member this.TestRand () =
        let t = dsharp.rand([1000])
        let tMean = t.mean()
        let tMeanCorrect = dsharp.tensor(0.5)
        let tStddev = t.stddev()
        let tStddevCorrect = dsharp.tensor(1./12.) |> dsharp.sqrt
        Assert.True(tMeanCorrect.allclose(tMean, 0.1))
        Assert.True(tStddevCorrect.allclose(tStddev, 0.1))

    [<Test>]
    member this.TestRandn () =
        let t = dsharp.randn([1000])
        let tMean = t.mean()
        let tMeanCorrect = dsharp.tensor(0.)
        let tStddev = t.stddev()
        let tStddevCorrect = dsharp.tensor(1.)
        printfn "%A %A" tMean tMeanCorrect
        printfn "%A %A" tStddev tStddevCorrect
        Assert.True(tMeanCorrect.allclose(tMean, 0.1, 0.1))
        Assert.True(tStddevCorrect.allclose(tStddev, 0.1, 0.1))

    [<Test>]
    member this.TestArange () =
        let t1 = dsharp.arange(5.)
        let t1Correct = dsharp.tensor([0.,1.,2.,3.,4.])
        let t2 = dsharp.arange(startVal=1., endVal=4.)
        let t2Correct = dsharp.tensor([1.,2.,3.])
        let t3 = dsharp.arange(startVal=1., endVal=2.5, step=0.5)
        let t3Correct = dsharp.tensor([1.,1.5,2.])
        Assert.AreEqual(t1Correct, t1)
        Assert.AreEqual(t2Correct, t2)
        Assert.AreEqual(t3Correct, t3)

    [<Test>]
    member this.TestSeed () =
        dsharp.seed(123)
        let t = dsharp.rand([10])
        dsharp.seed(123)
        let t2 = dsharp.rand([10])
        Assert.AreEqual(t, t2)

    [<Test>]
    member this.TestGrad () =
        let x = dsharp.tensor([1.5;2.5])
        let fx, g = dsharp.pgrad rosenbrock x
        let g2 = dsharp.grad rosenbrock x
        let fxCorrect = rosenbrock x
        let gCorrect = rosenbrockGrad x
        Assert.AreEqual(fxCorrect, fx)
        Assert.AreEqual(gCorrect, g)
        Assert.AreEqual(gCorrect, g2)

    [<Test>]
    member this.TestGradv () =
        let x = dsharp.tensor([1.5;2.5])
        let v = dsharp.tensor([2.75;-3.5])
        let fx, gv = dsharp.pgradv rosenbrock x v
        let gv2 = dsharp.gradv rosenbrock x v
        let fxCorrect = rosenbrock x
        let gvCorrect = dsharp.dot(rosenbrockGrad x,  v)
        Assert.AreEqual(fxCorrect, fx)
        Assert.AreEqual(gvCorrect, gv)
        Assert.AreEqual(gvCorrect, gv2)

    [<Test>]
    member this.TestJacobian () =
        let x = dsharp.arange(2.)
        let fx, j = dsharp.pjacobian fvect2vect2 x
        let j2 = dsharp.jacobian fvect2vect2 x
        let fxCorrect = fvect2vect2 x
        let jCorrect = fvect2vect2Jacobian x
        Assert.AreEqual(fxCorrect, fx)
        Assert.AreEqual(jCorrect, j)
        Assert.AreEqual(jCorrect, j2)

        let x = dsharp.arange(3.)
        let fx, j = dsharp.pjacobian fvect3vect2 x
        let j2 = dsharp.jacobian fvect3vect2 x
        let fxCorrect = fvect3vect2 x
        let jCorrect = fvect3vect2Jacobian x
        Assert.AreEqual(fxCorrect, fx)
        Assert.AreEqual(jCorrect, j)
        Assert.AreEqual(jCorrect, j2)

        let x = dsharp.arange(3.)
        let fx, j = dsharp.pjacobian fvect3vect4 x
        let j2 = dsharp.jacobian fvect3vect4 x
        let fxCorrect = fvect3vect4 x
        let jCorrect = fvect3vect4Jacobian x
        Assert.AreEqual(fxCorrect, fx)
        Assert.AreEqual(jCorrect, j)
        Assert.AreEqual(jCorrect, j2)