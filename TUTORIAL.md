Tutorial
========
This tutorial introduces introduces how to mint and send tokens on planet-node.

Creating Account
----------------

Before testing, you need to create a keystore if you don't have.
`planet-node` has sub-commands for this purpose.

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

Adjusting Genesis Action
------------------------

After creating key pair, you should add them to initial token distribution.
Token distribution has been defined as `InitalizeStates` action and you can
adjust it on `Program.cs` file.

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

Note: The `InitializeState` action is recorded in the genesis block and executed only once, so if the genesis was created by running planet-node before the change, you must delete the chain in the entire folder and start it for it to take effect.

Check the balance
-----------------
Minting proceeds automatically when the genesis block is created and executed. execute the node with the following command to check whether it has been successfully applied to the chain.

in bash
```bash
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

in PowerShell
```
$ $Env:PN_StorePath="/tmp/planet-node-chain"; dotnet run --project PlanetNode
```

Then, navigate to `http://localhost:38080/ui/playground` and type balance check query.

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

Transferring
------------

### Enabling mining

By default, planet-node doesn't start mining task(`MinerService<PlanetAction>`). to start the mining task, you need to feed different private key as miner key.

Execute bellow commands to generate new private key for miner.

```bash
$ dotnet run --project PlanetNode -- key generate
# Private Key and Address will be different.
Private key                                                      Address
---------------------------------------------------------------- ------------------------------------------
737b523d7d5594fabb1f37bbba712412034b02428568599ffec2ccc4a042ffc1 0x4b2fA0Fdf369364550259A351531cf410f43C111
```

As the name suggests, `key generate` command generates new private key for account. you can feed it to the `PN_MinerPrivateKeyString` environment variable or `appsettings.json` file.


in bash
```
$ export PN_StorePath=/tmp/planet-node-chain
$ export PN_MinerPrivateKeyString=737b523d7d5594fabb1f37bbba712412034b02428568599ffec2ccc4a042ffc1
```
in PowerShell
```
$ $Env:PN_StorePath="/tmp/planet-node-chain"
$ $Env:PN_MinerPrivateKeyString="737b523d7d5594fabb1f37bbba712412034b02428568599ffec2ccc4a042ffc1"
```
or
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

### Creating another account (as recipient)
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

To transfer money through GraphQL, execute the `transferAsset()` mutation. `transferAsset()` requires for the recipient's address and amount, and the sender's private key.

```graphql
transferAsset(
  recipient: String!
  amount: String!
  privateKeyHex: String!
): Transaction
```

To retreive private key for account(i.e. `25924579F8f1D6a0edE9aa86F9522e44EbC74C26`), use `key export` command.

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

Then go to the graphql playground(`http://localhost:38080/ui/playground`) again, execute bellow mutation. (Make sure the node is running as miner)

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

After new blocks, you can check the balances.

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
