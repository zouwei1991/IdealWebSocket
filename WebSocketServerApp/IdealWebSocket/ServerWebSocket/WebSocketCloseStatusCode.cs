using System;
using System.Collections.Generic;
using System.Text;

namespace IdealWebSocket.ServerWebSocket
{
    //websocket close status code defined in http://www.rfc-editor.org/rfc/rfc6455.txt senction 7
    public enum WebSocketCloseStatusCode
    {
        Normal=1000,
        EndpointUnavailable=1001,
        ProtocolError=1002,
        UnexceptedDataType=1003,
        Empty=1005,
        UnconsistentDataType=1007,
        PolicyViolation=1008,
        MessageTooBig=1009,
        ExtensionNegotiationFailure=1010,
        InternalServerError= 1011
        //0-999         Status codes in the range 0-999 are not used.
        //1000-2999     Status codes in the range 1000-2999 are reserved for definition by
        //              this protocol, its future revisions, and extensions specified in a
        //              permanent and readily available public specification.
        //3000-3999     Status codes in the range 3000-3999 are reserved for use by
        //              libraries, frameworks, and applications.  These status codes are
        //              registered directly with IANA.  The interpretation of these codes
        //              is undefined by this protocol.
        //4000-4999     Status codes in the range 4000-4999 are reserved for private use
        //              and thus can't be registered.  Such codes can be used by prior
        //              agreements between WebSocket applications.  The interpretation of
        //              these codes is undefined by this protocol.

    }
}
