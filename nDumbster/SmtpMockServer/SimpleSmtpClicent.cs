#region Using directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

#endregion
#region copyright
/*
 * nDumbster - a dummy SMTP server
 * Copyright 2005 Martin Woodward
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#endregion // copyright

namespace nDumbster.SmtpMockServer
{
    public class SimpleSmtpClicent
    {
        Socket _socket;
        SimpleSmtpServer _server;
        public SimpleSmtpClicent(SimpleSmtpServer server, Socket socket)
        {
            this._socket = socket;
            this._server = server;
        }

        public void HandleSmtpTransaction()
		{
            // Get the input and output streams
            NetworkStream networkStream = new NetworkStream(_socket);
            StreamReader input = new StreamReader(networkStream);
            StreamWriter output = new StreamWriter(networkStream);
			// Initialize the state machine
			SmtpState smtpState = SmtpState.CONNECT;
			SmtpRequest smtpRequest = new SmtpRequest(SmtpActionType.CONNECT, String.Empty, smtpState);

			// Execute the connection request
			SmtpResponse smtpResponse = smtpRequest.Execute();

			// Send initial response
			SendResponse(output, smtpResponse);
			smtpState = smtpResponse.NextState;

            List<SmtpMessage> msgList = new List<SmtpMessage>();
			SmtpMessage msg = new SmtpMessage();            
			while (smtpState != SmtpState.CONNECT)
			{
				string line = input.ReadLine();

				if (line == null)
				{
					break;
				}

				// Create request from client input and current state
				SmtpRequest request = SmtpRequest.CreateRequest(line, smtpState);
				// Execute request and create response object
				SmtpResponse response = request.Execute();
				// Move to next internal state
				smtpState = response.NextState;
				// Send reponse to client
				SendResponse(output, response);

				// Store input in message
				msg.Store(response, request.Params);

				// If message reception is complete save it
				if (smtpState == SmtpState.QUIT)
				{
					msgList.Add(msg);

                    MailMessageWrapper mmw = MailMessageWrapper.ConvertFrom(msg);
                    this._server.TriggerMessageReceived(mmw);                        
				    
				    msg = new SmtpMessage();
				}
			    if (request.GetAction()== SmtpActionType.EHLO)
			    {
			        msg.HostName = request.Params;
			    }
			}

            _socket.Close();
            _server.TriggerClientDisconnected();
            SimpleSmtpServer.ReceivedEmail.AddRange(msgList);
		}

		/// <summary>
		/// Send response to client.
		/// </summary>
		/// <param name="output">socket output stream</param>
		/// <param name="smtpResponse">Response to send</param>
		private void SendResponse(StreamWriter output, SmtpResponse smtpResponse)
		{
			if (smtpResponse.Code > 0)
			{
				output.WriteLine(smtpResponse.Code + " " + smtpResponse.Message);
				output.Flush();
			}
		}
    }
}
