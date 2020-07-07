namespace UdpToolkit.Core
{
    using System;

    public interface IDataGramBuilder
    {
        DataGram<TResp> All<TResp>(TResp response, byte hookId);

        DataGram<TResp> AllExcept<TResp>(TResp response, Guid peerId, byte hookId);

        DataGram<TResp> Room<TResp>(TResp response, byte roomId, byte hookId);

        DataGram<TResp> RoomExcept<TResp>(TResp response, byte roomId, Guid peerId, byte hookId);

        DataGram<TResp> Caller<TResp>(TResp response, byte roomId, Guid peerId, byte hookId);

        DataGram<TResp> Caller<TResp>(TResp response, Guid peerId, byte hookId);
    }
}