planet-node
===========

planet-node is a [.NET] CLI application as example for [Libplanet].
This application was developed as an example of blockchain node configuration using Libplanet, and is not suitable for production environment operation.

[Libplanet]: https://libplanet.io
[.NET]: https://docs.microsoft.com/en-US/dotnet/

Prerequisites
-------------

You need to install [.NET SDK] 6+. Read and follow the instruction to install
 .NET SDK on the [.NET download page][1].

[.NET SDK]: https://docs.microsoft.com/en-US/dotnet/core/sdk
[1]: https://dotnet.microsoft.com/en-us/download


Build
-----

```bash
$ git submodule update --init --recursive
$ dotnet build
```

If you want build docker, You can create a standalone image by running the command below.
```bash
$ docker build . -t <IMAGE_TAG>
```

How to Run
----------

```bash
$ dotnet run --project PlanetNode
```

### About configuration
Currently, planet-node produces and uses storage and settings via
`appsettings.json` and `PN_` prefixed environment variables. if you want to
change settings, please edit that files or set environment variables.

In sh/bash/zsh (Linux or macOS):

```sh
$ PN_StorePath="/tmp/planet-node" dotnet run --project PlanetNode
```

Or PowerShell (Windows):

```pwsh
PS > $Env:PN_StorePath="/tmp/planet-node"; dotnet run --project PlanetNode
```

### GraphQL
planet-node runs [GraphQL] server and [GraphQL Playground] automatically.
(backed by [GraphQL.NET]) you can check the current chain status on playground. (default endpoint is http://localhost:38080/ui/playground)

The following query is a GraphQL query that returns the last 10 blocks and
transactions.

```graphql
query
{
  explorer
  {
    blockQuery
    {
      blocks (limit: 10 desc: true)
      {
        index
        hash
        timestamp

        transactions
        {
          id
          actions
          {
            inspection
          }
        }
      }
    }
  }
}
```
<img width="919" alt="image" src="https://user-images.githubusercontent.com/128436/166613127-de83fd1d-7087-477e-9636-259aaa71f360.png">

Also, you can find supported GraphQL query in playground as like bellow.

<img width="478" alt="image" src="https://user-images.githubusercontent.com/128436/165906186-fc361126-f8f8-456a-bd28-fca938e60be1.png">

See the [Libplanet.Explorer] project for more details.
Also, if you want to try scenario based tutorial, please check the `TUTORIAL.md`.

Publish
-------

If you want to pack this project, use [`dotnet publish`][dotnet publish] as below.

```bash
$ dotnet publish -c Release --self-contained -r linux-x64
$ ls -al PlanetNode/bin/Release/net6.0/linux-x64/publish/
```

[dotnet publish]: https://docs.microsoft.com/en-US/dotnet/core/tools/dotnet-publish

[GraphQL]: https://graphql.org/
[GraphQL Playground]: https://github.com/graphql/graphql-playground
[GraphQL.NET]: https://graphql-dotnet.github.io/
[Libplanet.Explorer]: https://github.com/planetarium/libplanet/tree/main/Libplanet.Explorer
