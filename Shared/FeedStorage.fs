module FeedStorage

open Feed
open System
open System.IO
open Newtonsoft.Json
open System.Text

    let freeze path item =
        let encode (str:string) = Encoding.UTF8.GetBytes(str)
        async {
            try
                use fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None)
                let json = JsonConvert.SerializeObject(item) |> encode
                System.Diagnostics.Debug.WriteLine(json)
                do! fs.WriteAsync(json, 0, json.Length) |> Async.AwaitIAsyncResult |> Async.Ignore
                do! fs.FlushAsync() |> Async.AwaitIAsyncResult |> Async.Ignore
            with
            | e -> System.Diagnostics.Debug.WriteLine(e.ToString()); return ()
        }

     let thaw<'T> path =
        let decode buffer = Encoding.UTF8.GetString(buffer)
        async {
            try
                match File.Exists(path) with
                | false -> return None
                | true ->
                    use fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None)
                    let! buffer = fs.AsyncRead(fs.Length |> Convert.ToInt32)
                    let str = decode buffer
                    System.Diagnostics.Debug.WriteLine(str)
                    let bla = JsonConvert.DeserializeObject<'T>(str)

                    System.Diagnostics.Debug.WriteLine(typeof<'T>.GetType().FullName)
                    return bla |> Some
            with | e -> System.Diagnostics.Debug.WriteLine(e.ToString()); return None
        }

        