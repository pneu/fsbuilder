#light
(*
 * Machine Specified Environment
 *)
//let mage : string = @"C:\Program Files\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools\mage.exe"
let compiler : string = @"C:\Program Files (x86)\Microsoft SDKs\F#\3.1\Framework\v4.0\fsc.exe"
let fs_assembly_lib_path : string =
  @"C:\Program Files (x86)\Reference Assemblies\Microsoft\FSharp\.NETFramework\v4.0\4.3.1.0"
let assembly_lib_path : string =
  @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0"

(*
 * Project Environment
 *)
let common_option : string = "--nologo --noframework --tailcalls+ --warn:5"
type BuildOption = {
  source : string
  output : string
  options : string
  references : string list
  }

(*
 * Build Target
 * Note: When creation dll, specifies --target:library
 *       When creation exe, specifies --target:winexe or --taget:exe
 *       winexe for Windows application, exe for Console Application
 *)
let targetname : string = @"\changetime.exe"
let outputdir : string = @".\bin"
let buildItems : BuildOption list = [   // 依存されるアセンブリをHead側へ記述します.
    { source = @".\src\FileAttribute.AttributeChanger.fs"
      output = outputdir + @"\FileAttribute.AttributeChanger.dll"
      options = "--target:library --platform:anycpu"
      references = 
      [ fs_assembly_lib_path + @"\FSharp.Core.dll"
        assembly_lib_path + @"\System.dll"
      ]
    }
    { source = @".\src\main.fs"
      output = outputdir + targetname
      options = "--target:exe --platform:anycpu32bitpreferred --simpleresolution"
      references = 
      [ outputdir + @"\FileAttribute.AttributeChanger.dll"
        fs_assembly_lib_path + @"\FSharp.Core.dll"
        assembly_lib_path + @"\System.dll"
      ]
    }
  ]


(* logic *)
//type BuildContext =
//  | Debug of bool
//  | Normal of bool

let joinOptions (opt : BuildOption) =
  match opt with
  | { source = s; output = o; options = p; references = r } ->
    let quote e = "\"" + e + "\""
    let rs = List.fold (fun acc e -> acc + " -r " + (quote e)) "" r
    let j = System.String.Join (" ", [| p; "-o:"+ o; rs; s; |])
    j

let outputIfNotEmpty (e : bool) (x : string) =
  if System.String.IsNullOrWhiteSpace x then
    ()
  else
    if e then
      eprintfn "%s" x
    else
      printfn "%s" x

//let build (dcontext : BuildContext) =
let build (dcontext : bool) =
  for i in buildItems do
    let options = common_option + " " + (joinOptions i)
    let mutable procinfo = new System.Diagnostics.ProcessStartInfo(compiler, options)
    procinfo.RedirectStandardOutput <- true
    procinfo.RedirectStandardError <- true
    procinfo.UseShellExecute <- false
    let proc = System.Diagnostics.Process.Start procinfo
    proc.OutputDataReceived.Add (fun evArgs ->
      outputIfNotEmpty false evArgs.Data
      )
    proc.BeginOutputReadLine ()
    proc.StandardError.ReadToEnd () |> outputIfNotEmpty true
    proc.WaitForExit ()

let getArgs (args : string []) =
  //let opts = args.Tail
  //for o in opts do
  //  match o with
  //  | "-d" -> true
  //  | _ -> false
  //  |> ignore
  match Array.tryFind (fun e -> e = "-d") args with
  | Some(_) -> true
  | None -> false

//let DEBUG=
//let PDBNAME=
//echo %1
//if "%1"=="-d" then
//  set DEBUG=--debug:full
//  set PDBNAME=--pdb:%OUTPUT%.pdb
//)

(*
  Usage:
    Select two forms the below.
    1) > ProjectBuilder.main [||]
    2) > ProjectBuilder.main [|"-d"|]
*)
#if COMPILED
[<EntryPointAttribute>]
#endif
let main args =
  //let opt = getArgs fsi.CommandLineArgs
  //let opt = getArgs [||]
  build false
  //System.Console.ReadKey () |> ignore
  0

#if INTERACTIVE
[<EntryPointAttribute>]
main [||]
#endif
