Tutorial
========
This tutorial introduces how to mint and send tokens on planet-node.

Creating Account
----------------

Before testing, you need to create a keystore if there isn't one.
`planet-node` includes subcommands for this purpose.

```bash
$ dotnet run --project PlanetNode -- key

Building...
Key ID Address
------ -------

$ dotnet run --project PlanetNode -- key create
Building...
Passphrase: *
Retype passphrase: *
# Key ID and Address will be different.
Key ID                               Address
------------------------------------ ------------------------------------------
0be94e73-63a3-4ef8-b727-fad383726728 0x25924579F8f1D6a0edE9aa86F9522e44EbC74C26
```

The key is stored in:
- Linux/macOS: `$HOME/.config/planetarium/keystore`
- Windows: `%AppData%\planetarium\keystore`

Adjusting the Genesis Action
------------------------

After creating the key pair, you should include them for the initial token distribution.
The initial token distribution is done within the `InitalizeStates` action and you can
adjust the token distribution scheme in `Program.cs`.

```csharp
    builder.Services
        .AddLibplanet<PolymorphicAction<PlanetAction>>(
            headlessConfig,
            new PolymorphicAction<PlanetAction>[]
            {
                new InitializeStates(
                    new Dictionary<Address, FungibleAssetValue>
                    {
                        // Replace with your account address (no "0x" prefix).
                        // 1000 is mint amount.
                        [new Address("25924579F8f1D6a0edE9aa86F9522e44EbC74C26")] = Currencies.PlanetNodeGold * 1000,
                    }
                )
            }
        )
```

Note: The `InitializeState` action is recorded in the genesis block and is executed only once, so if there is already a genesis block created by running planet-node before a change was made in the token distribution scheme, you must delete the entire directory containing the chain and re-run planet-node for the new scheme to take place.

Check the balance
-----------------
Tokens that are included in the initial distribution scheme are minted automatically when the genesis block is created and executed. To verify that the genesis block generation and the token distribution has been successful, do the following:

In sh/bash/zsh (Linux or macOS):

```sh
$ dotnet run --project PlanetNode
Building...
warn: Microsoft.AspNetCore.Server.Kestrel[0]
      Overriding address(es) 'https://localhost:7040, http://localhost:5005'. Binding to endpoints defined via IConfiguration and/or UseKestrel() instead.
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://0.0.0.0:38080
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
info: Microsoft.Hosting.Lifetime[0]
      Content root path: /home/longfin/planet-node/PlanetNode/
```

Or in PowerShell (Windows):

```
PS > dotnet run --project PlanetNode
```

Then, navigate to the GraphQL Playground at `http://localhost:38080/ui/playground` in a web browser and execute the following query to check the balance:

```gql
query
{
  application
  {
    asset(address: "25924579F8f1D6a0edE9aa86F9522e44EbC74C26")
  }
}
```

<img width="960" alt="image" src="https://user-images.githubusercontent.com/128436/166153745-7707d3a4-ece8-4ce6-a9ef-38c430fce603.png">

Transferring Assets
-------------------

### Enabling mining

By default, planet-node does not start the mining task (`MinerService<PlanetAction>`). To start the miner, you need to provide it with a private key.

Execute the following commands to generate a new private key for the miner.

```bash
$ dotnet run --project PlanetNode -- key generate
# Private Key and Address will be different.
Private key                                                      Address
---------------------------------------------------------------- ------------------------------------------
737b523d7d5594fabb1f37bbba712412034b02428568599ffec2ccc4a042ffc1 0x4b2fA0Fdf369364550259A351531cf410f43C111
```

As the name suggests, the `key generate` command generates a private key for a new account. You can feed it to the miner through the `PN_MinerPrivateKeyString` environment variable or the `appsettings.json` file.


In sh/bash/zsh (Linux or macOS):

```sh
$ export PN_MinerPrivateKeyString=737b523d7d5594fabb1f37bbba712412034b02428568599ffec2ccc4a042ffc1
```

Or in PowerShell (Windows):

```pwsh
PS > $Env:PN_MinerPrivateKeyString="737b523d7d5594fabb1f37bbba712412034b02428568599ffec2ccc4a042ffc1"
```

Or in the `appsettings.json` file:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  // other settings
  "MinerPrivateKeyString": "737b523d7d5594fabb1f37bbba712412034b02428568599ffec2ccc4a042ffc1"
}
```

### Creating another account (as the recipient)
Now let's create another account to receive the tokens. Just use the `key create` command as we did before.

```bash
$ dotnet run --project PlanetNode -- key create
Building...
Passphrase: *
Retype passphrase: *
# Key ID and Address will be different.
Key ID                               Address
------------------------------------ ------------------------------------------
d8576720-c11a-44ab-9282-9661531f9438 0xA9Ce73B2B1EB603A10A6b50CF9f37fBa59e7a79A
```

### Execute mutation

To transfer tokens through GraphQL, execute the `transferAsset()` mutation. `transferAsset()` asks for the recipient's address, the amount to transfer, and the sender's private key.

```graphql
transferAsset(
  recipient: String!
  amount: String!
  privateKeyHex: String!
): Transaction
```

To retrieve the private key for the sending account(i.e. `25924579F8f1D6a0edE9aa86F9522e44EbC74C26`), use the `key export` command.

```bash
# Check the Key ID for account
$ dotnet run --project PlanetNode -- key
Building...
Key ID                               Address
------------------------------------ ------------------------------------------
0be94e73-63a3-4ef8-b727-fad383726728 0x25924579F8f1D6a0edE9aa86F9522e44EbC74C26
d8576720-c11a-44ab-9282-9661531f9438 0xA9Ce73B2B1EB603A10A6b50CF9f37fBa59e7a79A

$ dotnet run --project PlanetNode -- key export 0be94e73-63a3-4ef8-b727-fad383726728
Building...
Passphrase (of 0be94e73-63a3-4ef8-b727-fad383726728): *
924a03ecd4a56db981005d3338dfc78dfee673112aa95781b0a5d9668afe3ecd
```

Then, go to the GraphQL Playground again (`http://localhost:38080/ui/playground`), and execute the following mutation. (Make sure the node is running as a miner)

```graphql
mutation
{
  transferAsset(
    recipient: "A9Ce73B2B1EB603A10A6b50CF9f37fBa59e7a79A"
    amount: "50"
    privateKeyHex: "924a03ecd4a56db981005d3338dfc78dfee673112aa95781b0a5d9668afe3ecd"
  )
  {
    id
  }
}
```
<img width="998" alt="image" src="https://user-images.githubusercontent.com/128436/166154488-47dd6056-a260-4667-9d4b-eb4a2acabf1a.png">

After a block is mined, you can check the balances.

```graphql
query
{
  application
  {
    asset(address: "A9Ce73B2B1EB603A10A6b50CF9f37fBa59e7a79A")
  }
}
```
<img width="896" alt="image" src="https://user-images.githubusercontent.com/128436/166154562-4e471e6d-c5d5-443a-9893-c05c6ba3aa9c.png">


Transferring using wallet
-------------------------
In the previous section, we provided the private key of the sending account along with the mutation to the GraphQL endpoint to conduct asset transfer. However, in general, transmitting an account private key to the outside world is very dangerous because in case it is exposed, it can be used to gain complete access to the account, and it cannot be revoked or renewed. Instead, many blockchain network users prefer signing their transactions with a special piece of software (known as a "Crypto Wallet") and transmitting the signed transaction.

In this section, we will learn how to send assets step-by-step with [pn-chrono], the sample crypto wallet software.

[pn-chrono]: https://github.com/planetarium/pn-chrono

### Prerequisites

First, you need to get the pn-chrono project. It is served on GitHub as well and you can check the detailed instruction to build and run on [README.md][pn-chrono's README].

[pn-chrono's README]: https://github.com/planetarium/pn-chrono/blob/0e25b226ae671de4d1b9f76570af1adbb3f8c6e3/README.md

### Import account

1. After you succeed to build and run pn-chrono, you will see the UI, as Google Chrome extensions as below.

    <img width="286" alt="image" src="https://user-images.githubusercontent.com/128436/189905396-25c586f0-a75e-443f-8bd4-2a77b8858c68.png">

2. Click on the "New" tab and Create a temporary account to proceed. (we don't use this account)

    <img width="298" alt="image" src="https://user-images.githubusercontent.com/128436/189906608-e1e5b0da-7737-4e8a-9eac-1a5d84b5903f.png">

3. Click "Account" and "Import".

    <img width="287" alt="image" src="https://user-images.githubusercontent.com/128436/189906828-7e9e40c7-b6c9-4a5c-8aee-57aa80ddcb37.png">

4. Paste the raw private key (hex string) to the pane and click "Import".

    <img width="250" alt="image" src="https://user-images.githubusercontent.com/128436/189907053-fcb82f2f-6eb9-4cce-8f93-ff15196b304d.png">

5. Check if the address is the same as displayed on the `planet` CLI.

    <img width="256" alt="image" src="https://user-images.githubusercontent.com/128436/189907626-af72a817-bb0b-487c-ad47-c19882399083.png">

### Sign to transfer

1. Click "Transfer" on the main view.

    <img width="281" alt="image" src="https://user-images.githubusercontent.com/128436/189909254-c68ad7b6-3ccb-4030-a580-7cf26e736251.png">

2. Fill the "Receiver" and "Amount" fields.

    <img width="281" alt="image" src="https://user-images.githubusercontent.com/128436/189910018-b81baa7c-8e24-4fbd-9b1e-f8d447cb4bbc.png">

3. Check the transaction detail once again and Click "Transfer".

    <img width="281" alt="image" src="https://user-images.githubusercontent.com/128436/189910227-3c671b1b-6bbe-4c71-9e8d-77e34ab34017.png">

4. Wait for the next block and check the balance.


Going Multinode
---------------

One of the important features of a blockchain software to achieve decentralization is the ability to establish independent nodes and have them connect to each other, synchronizing the chain state. Libplanet offers the `Swarm<T>` class in Libplanet.Net package, which provides functionality to manage synchronization with peer nodes.

In this section, we learn how to run multiple instances of planet-node that are connected and synchronized with one another. For this purpose, we provide appsettings.peer.json to supply the example settings for the second node. For simplicity, we will enable mining only on the first node.

### Copying the blockchain

First of all, every node of the same blockchain must share at least the genesis block. To achieve this, we will copy the planet-node directory which contains the chain on disk to the planet-node-a directory, which is designated as the store directory in appsettings.peer.json. (Note that when using `dotnet run`, all relative path is resolved relative to the project path, which in this case is the PlanetNode/ directory.) Assuming that you already have initialized the chain and enabled mining on the first node (stop the node first if it is running):

In sh/bash/zsh (Linux or macOS):

```sh
$ cp -r PlanetNode/planet-node-chain{,-a}
```

Or in PowerShell (Windows):

```pwsh
PS > Copy-Item -Path PlanetNode/planet-node-chain -Destination PlanetNode/planet-node-chain-a -recurse
```

### Acquiring the peer string

For a node to be able to connect to other nodes, it should be aware about the whereabouts of at least a single node connected to the blockchain network. Oftentimes in blockchain networks, there exist some specialized nodes (called the seed nodes) with well-known addresses which provide convenient points for individual nodes to initiate a connection to the network. Since we're establishing ourselves a new blockchain network, we need to provide ourselves with a "seed node" that can be used for other nodes to connect to the network. The specification for a peer is represented with a "peer string", which we have to retrieve from our seed node. We're going to be using our miner node as the seed node, but note that any node exposed to the public network is eligible to be a seed node.

There are currently two methods to retrieve the peer string: from the console log, and with a GraphQL query.

#### Getting the peer string from the console log

Start the miner node and watch the log for the peer string of the node. It should be near the beginning of the log:

```sh
$ dotnet run --project PlanetNode
```

```
[12:23:31 INF] [NetMQTransport] Listening on 31234...
[12:23:31 DBG] [Swarm] Tip before preloading: #1387 374ff37ead1d1a5d0afb9686a9557256cdda627532261f937d3eda250c311f03
[12:23:31 INF] [Swarm] Fetching excerpts from 0 peers...
[12:23:31 INF] [Swarm] There are no appropriate peers for preloading.
[12:23:31 DBG] [Swarm] Starting swarm...
[12:23:31 DBG] [Swarm] Peer information : 0x558CfD1a5e8D87d1116e5798736b52B100EF46E3.Unspecified/localhost:31234.
[12:23:31 DBG] [Swarm] Watching the BlockChain for tip changes...
warn: Microsoft.AspNetCore.Server.Kestrel[0]
      Overriding address(es) 'https://localhost:7040, http://localhost:5005'. Binding to endpoints defined via IConfiguration and/or UseKestrel() instead.
peerString: 035e91ac972827567226c3595b6cd942407ee64043b8760a72f2a051ebc6229d66,localhost,31234.
```

Here, `035e91ac972827567226c3595b6cd942407ee64043b8760a72f2a051ebc6229d66,localhost,31234` is the peer string. Notice that the trailing period is not included.

#### Querying the node for the peer string

Alternatively, you can query the node with GraphQL for the peer string. Start the miner node, go to the GraphQL Playground of the miner node at http://localhost:38080/ui/playground, and execute the query:

```graphql
query{
  application {
    peerString
  }
}
```

The node will return the peer string along the result, similar to the following:

```json
{
  "data": {
    "application": {
      "peerString": "035e91ac972827567226c3595b6cd942407ee64043b8760a72f2a051ebc6229d66,localhost,31234."
    }
  }
}
```

Again, note that the trailing period is not a part of the peer string.

### Supplying the peer string of the miner node to the second node

Now we provide the peer string to the `appsettings.peer.json` of the second node:

```json
{
  // ...
  "PeerStrings": [
    "035e91ac972827567226c3595b6cd942407ee64043b8760a72f2a051ebc6229d66,localhost,31234"
  ],
  // ...
}
```

Then, we can finally start the second node.

### Starting the second node

While the first node is still running, start the second node from a different terminal, providing planet-node with a path to the config file for the second node in PN_CONFIG_FILE environment variable (again, note that relative path is resolved relative to the project path \[PlanetNode/\] when using dotnet run):

In sh/bash/zsh (Linux or macOS):

```sh
$ PN_CONFIG_FILE=appsettings.peer.json dotnet run --project PlanetNode
```

Or in PowerShell (Windows):

```pwsh
PS > $Env:PN_CONFIG_FILE="appsettings.peer.json"; dotnet run --project PlanetNode; Remove-Item Env:\PN_CONFIG_FILE
```

Now, on the console log of the second node that has just been set up, you will see some entries like the following:

```
[18:25:59 DBG] [NetMQTransport] Trying to send request Libplanet.Net.Messages.PingMsg 5a3090ae-937a-4d94-adb9-76865798796b to 0xBD465F970c201DFaeDA6de16E83724d8E04c1dDF.Unspecified/localhost:31234. with timeout null...
[18:25:59 DBG] [NetMQTransport] Request Libplanet.Net.Messages.PingMsg 5a3090ae-937a-4d94-adb9-76865798796b sent to 0xBD465F970c201DFaeDA6de16E83724d8E04c1dDF.Unspecified/localhost:31234..
[18:25:59 DBG] [NetMQTransport] A reply to request Libplanet.Net.Messages.PingMsg 5a3090ae-937a-4d94-adb9-76865798796b from 0x4F7e5F90E02Ad240637a57aF079c896E69884bCa.Unspecified/localhost:31234. has parsed: Libplanet.Net.Messages.PongMsg.
[18:25:59 DBG] [NetMQTransport] Request Libplanet.Net.Messages.PingMsg 5a3090ae-937a-4d94-adb9-76865798796b with timeout 0ms processed in 144ms with 1 replies received out of 1 expected replies.
[18:25:59 DBG] [NetMQTransport] Received 1 reply messages to 5a3090ae-937a-4d94-adb9-76865798796b from 0xBD465F970c201DFaeDA6de16E83724d8E04c1dDF.Unspecified/localhost:31234.: ["Libplanet.Net.Messages.PongMsg"].
[18:25:59 DBG] [RoutingTable] Adding peer 0xBD465F970c201DFaeDA6de16E83724d8E04c1dDF.Unspecified/localhost:31234. to the routing table...
```

You can see here that the second node is pinging the miner node, and then adds the miner peer to the routing table after the miner peer responds in a pong message. Then, the second peer does the following:

 * Requests for the chain status from the miner node
 * Sees that it needs to catch up on the chain
 * Asks for new blocks before starting to watch for changes in the chain ("preloading")
 * Get the new block hashes and block content, and apply them to the current chain
 * Execute the actions in the new blocks
 * Then, starts the swarm and watch for tip changes in the chain.

You can also see the miner node respond in a corresponding manner. Now the second node is up and running, and follows the changes in the chain as it moves forward.

### Verifying if the second node is functional

In appsettings.peer.json, the second node is set to use 38081/tcp port for the GraphQL server. We will query the two nodes for the same block to see if the nodes are in sync.

Go to the GraphQL Playground of the miner node at http://localhost:38080/ui/playground, and query for the tip:

```gql
query
{
  explorer {
    blockQuery {
      blocks(desc: true, offset: 0, limit: 1)
      {
        hash
        index
      }
    }
  }
}
```

It should respond in the following manner:

```json
{
  "data": {
    "explorer": {
      "blockQuery": {
        "blocks": [
          {
            "hash": "03eb151050efabe9436c12f45efa6e9a67da2d4be6a8870025001cb4fcba2618",
            "index": 777
          }
        ]
      }
    }
  }
}
```

Take note of the block hash. Now, go to the GraphQL Playground in the second node at http://localhost:38081/ui/playground and query for the block that has the block hash:

```gql
query {
  explorer {
    blockQuery {
   	  block(hash: "03eb151050efabe9436c12f45efa6e9a67da2d4be6a8870025001cb4fcba2618")
      {
        index
      }
    }
  }
}
```

You can see in the result that the block is on the same height in the second node as the miner node:

```json
{
  "data": {
    "explorer": {
      "blockQuery": {
        "block": {
          "index": 777
        }
      }
    }
  }
}
```

From these results, we can confirm that the second node is successfully running and receiving new blocks from the miner node.

### Querying the chain on the second node

As with the miner node, we can also query the second node for data on the chain. In the GraphQL Playground of the second node at http://localhost:38081/ui/playground:

```gql
query
{
  application
  {
    asset(address: "25924579F8f1D6a0edE9aa86F9522e44EbC74C26")
  }
}
```

You can see that it successfully retrieves the balance:

```json
{
  "data": {
    "application": {
      "asset": "950 PNG"
    }
  }
}
```

### Mutation on the second node

Although the second node does not have the miner running, it should be able to broadcast the signed transaction to the miner node and have the transaction included in a block, changing the chain state. We can demonstrate this by initiating an asset transfer on the second node:

On the GraphQL Playground of the second node at http://localhost:38081/ui/playground:

```graphql
mutation
{
  transferAsset(
    recipient: "A9Ce73B2B1EB603A10A6b50CF9f37fBa59e7a79A"
    amount: "50"
    privateKeyHex: "924a03ecd4a56db981005d3338dfc78dfee673112aa95781b0a5d9668afe3ecd"
  )
  {
    id
  }
}
```

In the result of this mutation, you can retrieve the transaction id.

```json
{
  "data": {
    "transferAsset": {
      "id": "d9a5d7638350f3b38cde81772f7bf147ac67a73b20893922ae89ddaeb52c246f"
    }
  }
}
```

To see if this transaction is successfully included in a block, query the second node for the transaction result for the transaction id:

```graphql
query
{
  explorer
  {
    transactionQuery
    {
      transactionResult (txId:"d9a5d7638350f3b38cde81772f7bf147ac67a73b20893922ae89ddaeb52c246f")
      {
        txStatus
        blockIndex
        blockHash
      }
    }
  }
}
```

If the transaction is in a block on the chain, it will respond in the following manner:

```json
{
  "data": {
    "explorer": {
      "transactionQuery": {
        "transactionResult": {
          "txStatus": "SUCCESS",
          "blockIndex": 1554,
          "blockHash": "4ecdb169992d8c15522845fd08368d32273042726ad83f8de70050d24404fbfe"
        }
      }
    }
  }
}
```

Now you can query the recipient's balance to see if the asset has been successfully transferred:

```graphql
query
{
  application
  {
    asset(address: "A9Ce73B2B1EB603A10A6b50CF9f37fBa59e7a79A")
  }
}
```

You will get a response like the following if the transfer was successfully made.

```json
{
  "data": {
    "application": {
      "asset": "100 PNG"
    }
  }
}
```

Now that we can see that the nodes are working in tandem, you can go crazy and try out other network structures by establishing other nodes and connecting them to each other!
