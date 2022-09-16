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
# chain will be stored under `/tmp/planet-node-chain` directory.
$ PN_StorePath=/tmp/planet-node-chain dotnet run --project PlanetNode
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

Or PowerShell (Windows):

```
PS > $Env:PN_StorePath="/tmp/planet-node-chain"; dotnet run --project PlanetNode
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
$ export PN_StorePath=/tmp/planet-node-chain
$ export PN_MinerPrivateKeyString=737b523d7d5594fabb1f37bbba712412034b02428568599ffec2ccc4a042ffc1
```

Or PowerShell (Windows):

```pwsh
PS > $Env:PN_StorePath="/tmp/planet-node-chain"
PS > $Env:PN_MinerPrivateKeyString="737b523d7d5594fabb1f37bbba712412034b02428568599ffec2ccc4a042ffc1"
```

Or the `appsettings.json` file:

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


Communicating between multiple nodes
------------------------------------

### Before you begin

Please check where the store of two nodes are located before starting. The location of a store for a node instance can be given as an environment variable PN_StorePath.
We will be using the following paths:

> Location of the store for the first node: /tmp/planet-node-chain  
> Location of the store for the second node: /tmp/planet-node-chain-a

### Copying the store of the first node for the second node so that they share the common genesis block
```shell
$ cp -r /tmp/planet-node-chain /tmp/planet-node-chain-a
```

### Starting the first node
```shell
$ PN_StorePath=/tmp/planet-node-chain dotnet run --project PlanetNode
```

If you would like to verify that two nodes share the same genesis block, you might be able to modify the code so that the nodes output the hash string of the genesis block as below, in planet-node/Libplanet.Headless/Hosting/SwarmService.cs:

```cs
protected override async Task ExecuteAsync(CancellationToken stoppingToken)

{

 _ = _swarm.WaitForRunningAsync().ContinueWith(_ =>

 {

  var peer = _swarm.AsPeer;
  var result = getPeerString(peer);
  Console.WriteLine("Genesis hash: {0}", _swarm.BlockChain.Genesis.Hash); // use Console.WriteLine
});

 await _swarm.AddPeersAsync(_peers, default, cancellationToken: 
 stoppingToken).ConfigureAwait(false);
 await _swarm.PreloadAsync(cancellationToken: stoppingToken).ConfigureAwait(false);
 await _swarm.StartAsync(cancellationToken: stoppingToken).ConfigureAwait(false);
}
```

### Copying the peer string of the first node and providing it to the `PeerStrings` array in `appsettings.peer.json`

**Querying the first node for the peer string** 

Go the the GraphQL Playground of the first node at http://localhost:38080/ui/playground and execute the query:

```graphql
query{
  application {
    peerString
  }
}
```

Example result:
```json
{
  "data": {
    "application": {
      "peerString": "034f11693177d1a3d7a20c10cf064fd89f82592cd8fd2b25bc8af2855878e831b9,localhost,31234."
    }
  }
}
```

Please note that the trailing period must be removed:
```
Incorrect: "034f11693177d1a3d7a20c10cf064fd89f82592cd8fd2b25bc8af2855878e831b9,localhost,31234."
Correct: "034f11693177d1a3d7a20c10cf064fd89f82592cd8fd2b25bc8af2855878e831b9,localhost,31234"
```


In appsettings.peer.json:
```json
{
 // ...
 "PeerStrings": ["034f11693177d1a3d7a20c10cf064fd89f82592cd8fd2b25bc8af2855878e831b9,localhost,31234"],
 // ...
}
```

### Starting the second node

```shell
$ PN_StorePath=/tmp/planet-node-chain-a PN_CONFIG_FILE=appsettings.peer.json  dotnet run --project PlanetNode
```

### Creating a transaction in the second node & Checking in the first node

```shell
$ dotnet run --project PlanetNode -- key

#Key ID                               Address                                   
#------------------------------------ ------------------------------------------
#b53ba868-4749-49d3-b2aa-2433a507370b 0x50129015Fa9F02AE2db055d4C675E42f5Ec82066
#62ca49b2-999b-4459-a9a9-6fb64317864c 0xBc35F4797514e6f13736e6C6777BAea4764B0526
#9f5a70fd-b9b2-415f-8ff2-eb64cc7629f7 0x917955C717b82801479e43732BD9b3c6710e5000
#66aa63b4-048a-43f7-a747-e769bf861f36 0xBd04842e6Bee1b6399F143D2CeB65EAE8f7ee453

$ dotnet run --project PlanetNode -- key export 62ca49b2-999b-4459-a9a9-6fb64317864c

#Passphrase (of 62ca49b2-999b-4459-a9a9-6fb64317864c): 
#543140f03e7294eea41c67efbcca6f20c2f557e0a6101f8a6935ffc0aec9bcae
```

Go to the GraphQL Playground of the second node at http://localhost:38081/ui/playground and execute the mutation:

```graphql
mutation
{
  transferAsset(
    recipient: "917955C717b82801479e43732BD9b3c6710e5000"
    amount: "50"
    privateKeyHex: "543140f03e7294eea41c67efbcca6f20c2f557e0a6101f8a6935ffc0aec9bcae"
  )
  {
    id
  }
}
``` 

Example execution result of the mutation:
```json
{
  "data": {
    "transferAsset": {
      "id": "d9a5d7638350f3b38cde81772f7bf147ac67a73b20893922ae89ddaeb52c246f"
    }
  }
}
```

Go to the GraphQL Playground of the first node at http://localhost:38080/ui/playground and execute the query:

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

Example result:
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
