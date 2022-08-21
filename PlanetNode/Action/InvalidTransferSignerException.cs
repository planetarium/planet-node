// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Libplanet;
using System.Runtime.Serialization;

namespace PlanetNode.Action;
[Serializable]
public class InvalidTransferSignerException : Exception, ISerializable
{
    private readonly Address _signer;
    private readonly Address _sender;
    private readonly Address _recipient;

    public InvalidTransferSignerException()
    {
    }

    public InvalidTransferSignerException(string? message)
        : base(message)
    {
    }

    public InvalidTransferSignerException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    public InvalidTransferSignerException(Address signer, Address sender, Address recipient)
    {
        _signer = signer;
        _sender = sender;
        _recipient = recipient;
    }

    protected InvalidTransferSignerException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        if (info.GetString(nameof(_signer)) is { } signerHex)
        {
            _signer = new Address(signerHex);
        }

        if (info.GetString(nameof(_sender)) is { } senderHex)
        {
            _sender = new Address(senderHex);
        }

        if (info.GetString(nameof(_recipient)) is { } recipientHex)
        {
            _recipient = new Address(recipientHex);
        }
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);

        info.AddValue(nameof(_signer), _signer.ToHex());
        info.AddValue(nameof(_sender), _sender.ToHex());
        info.AddValue(nameof(_recipient), _recipient.ToHex());
    }
}
