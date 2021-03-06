open Silk.NET.Windowing
open System.Threading
open Silk.NET.OpenGL
open Microsoft.FSharp.NativeInterop
open System
open System.IO
open System.Drawing
open Data
open System.Numerics
open System.Runtime.InteropServices

let compileShader (shader: uint32) (gl: GL) =
    gl.CompileShader(shader)

    let code =
        gl.GetShader(shader, GLEnum.CompileStatus)

    if code <> int GLEnum.True then
        failwith (sprintf "Error compiling shader %d" shader)

let linkProgram (program: uint32) (gl: GL) =
    gl.LinkProgram(program)

    let code =
        gl.GetProgram(program, GLEnum.LinkStatus)

    if code <> int GLEnum.True then
        failwith (sprintf "Error linking program %d" program)

let createShader vertPath fragPath (gl: GL) =
    let vertexShaderSource = File.ReadAllText vertPath
    let fragmentShaderSource = File.ReadAllText fragPath
    let vertexShader = gl.CreateShader(GLEnum.VertexShader)
    let fragmentShader = gl.CreateShader(GLEnum.FragmentShader)
    gl.ShaderSource(vertexShader, vertexShaderSource)
    gl.ShaderSource(fragmentShader, fragmentShaderSource)
    compileShader vertexShader gl
    compileShader fragmentShader gl

    let handle = gl.CreateProgram()
    gl.AttachShader(handle, vertexShader)
    gl.AttachShader(handle, fragmentShader)

    linkProgram handle gl

    gl.DetachShader(handle, vertexShader)
    gl.DetachShader(handle, fragmentShader)
    gl.DeleteShader(vertexShader)
    gl.DeleteShader(fragmentShader)

    handle

[<EntryPoint>]
let main _ =

    let mutable options = WindowOptions.Default
    options.UpdatesPerSecond <- 60.0
    options.FramesPerSecond <- 60.0

    let window = Window.Create(options)
    window.Title <- "Luigi #1"

    window.add_Load
        (fun () ->
            let gl = GL.GetApi(window)

            let vertexBufferObject = gl.GenBuffer()
            let vertexArrayObject = gl.GenVertexArray()

            let shaderHandle =
                createShader "shader.vert" "shader.frag" gl

            gl.ClearColor(0.2f, 0.3f, 0.3f, 1.0f)
            gl.BindBuffer(GLEnum.ArrayBuffer, vertexBufferObject)

            let teapotItemCount = teapot.Length

            let teapotStackAllocation =
                NativePtr.stackalloc<float32> (teapotItemCount)

            let teapotAllocationAddress =
                teapotStackAllocation |> NativePtr.toVoidPtr

            let size =
                unativeint (teapot.Length * sizeof<float32>)

            gl.BufferData(GLEnum.ArrayBuffer, size, teapotAllocationAddress, GLEnum.StaticDraw)

            let uniformLocation =
                gl.GetUniformLocation(shaderHandle, "matrix")

            let foo = 

            let translationMatrix =
                new ReadOnlySpan<float32>(
                    [| 1.0f
                       1.0f
                       0.0f
                       1.0f
                       1.0f
                       0.0f
                       1.0f
                       1.0f
                       0.0f |]
                )

            gl.UniformMatrix4(uniformLocation, false, translationMatrix)
            gl.UseProgram(shaderHandle)
            gl.BindVertexArray(vertexArrayObject)
            gl.VertexAttribPointer(0u, 3, GLEnum.Float, false, uint32 (3 * sizeof<float32>), IntPtr.Zero.ToPointer())
            gl.EnableVertexAttribArray(0u)
            gl.BindBuffer(GLEnum.ArrayBuffer, vertexBufferObject)

            window.add_Render
                (fun _ ->
                    gl.Clear(uint32 GLEnum.ColorBufferBit)
                    gl.UseProgram(shaderHandle)
                    gl.BindVertexArray(vertexArrayObject)
                    gl.DrawArrays(GLEnum.Triangles, 0, 3u))

            window.add_Resize (fun size -> gl.Viewport(size))

            window.add_Closing
                (fun () ->
                    gl.BindBuffer(GLEnum.ArrayBuffer, 0u)
                    gl.BindVertexArray(0u)
                    gl.UseProgram(0u)
                    gl.DeleteBuffer(vertexBufferObject)
                    gl.DeleteVertexArray(vertexArrayObject)
                    gl.DeleteProgram(shaderHandle)))

    window.Run()
    0 // exit sucessfully
