#region Using directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

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
    public class SimpleSmtpServer : IDisposable
	{

        public delegate void MessageReceivedHandler(MailMessageWrapper mmw);
	    public event MessageReceivedHandler OnMessageReceived;

        #region Members
        /// <summary>
        /// Default SMTP port is 25.
        /// </summary>
        public const string DEFAULT_SMTP_HOST_IP = "127.0.0.1";

        public const int DEFAULT_SMTP_PORT = 25;

        private string HostIp { get; set; }

	    /// <summary>
		/// Stores the port that the SmtpServer listens to
		/// </summary>
		private int port = SimpleSmtpServer.DEFAULT_SMTP_PORT;
		
	
		/// <summary>
		/// Indicates whether this server is stopped or not.
		/// </summary>
		private volatile bool stopped = true;

		/// <summary>
		/// Listen for client connection
		/// </summary>
		protected TcpListener tcpListener = null;

		/// <summary>
		/// Synchronization <see cref="AutoResetEvent">event</see> : Set when server has started (successfully or not)
		/// </summary>
		internal AutoResetEvent startedEvent = null;

		/// <summary>
		/// Last <see cref="Exception">Exception</see> that happened in main loop thread
		/// </summary>
		internal Exception mainThreadException = null;

		#endregion // Members

		#region Contructors
		/// <summary>
		/// Constructor.
		/// </summary>
		private SimpleSmtpServer(string hostIp, int port)
		{
		    this.HostIp = hostIp;
			this.port = port;
			this.startedEvent = new AutoResetEvent(false);
		}
		#endregion // Constructors;

		#region Properties
		/// <summary>
		/// Indicates whether this server is stopped or not.
		/// </summary>
		/// <value><see langword="true"/> if the server is stopped</value>
		virtual public bool Stopped
		{
			get
			{
				return stopped;
			}
		}
		
		/// <summary>
		/// The port that the SmtpServer listens to
		/// </summary>
		/// <value>Port used to accept client connections</value>
		public int Port
		{
			get
			{
				return this.port;
			}
		}

        private static List<SmtpMessage> receivedEmail;
		public static List<SmtpMessage> ReceivedEmail
		{
            get
            {
                if (receivedEmail == null)
                {
                    receivedEmail = new List<SmtpMessage>();
                }
                return receivedEmail;
            }
		}

		#endregion  // Properties

		/// <summary>
		/// Main loop of the SMTP server.
		/// </summary>
		internal void Run()
		{
			stopped = false;
			try
			{
				try
				{
                    // Open a listener to accept client connection
                    tcpListener = new TcpListener(IPAddress.Any, Port);                    
                    //tcpListener = new TcpListener(IPAddress.Parse(HostIp), Port);				    
                    tcpListener.Start();
				}
				catch(Exception e)
				{
					// If we can't start the listener, we don't start loop
					stopped = true;
					// And store exception that will be thrown back to the thread
					// that started the server
					mainThreadException = e;
				}
				finally
				{
					// Inform calling thread that we can noew receive messages
					// or that something bad happened.
					startedEvent.Set();
				}
                int numberOfClients = 0;
                // Server: loop until stopped
                while (!Stopped)
                {
                    Socket socket = null;
                    try
                    {
                        // Accept an incomming client connection
                        socket = tcpListener.AcceptSocket();
                    }
                    catch
                    {
                        if (socket != null)
                        {
                            socket.Close();
                        }
                        continue; // Non-blocking socket timeout occurred: try accept() again
                    }
                    this.TriggerClientConnected();
                    SimpleSmtpClicent client = new SimpleSmtpClicent(this, socket);
                    Thread myThread = new Thread(new ThreadStart(client.HandleSmtpTransaction));
                    numberOfClients += 1;
                    myThread.IsBackground = true;
                    myThread.Start();
                }
			}
			catch (Exception e)
			{
				// Send exception back to calling thread
				mainThreadException = e;
			}
			finally
			{
				// The server won't listen anymore
				stopped = true;

				// Stop the listener if it was started
				if (tcpListener != null)
				{
					tcpListener.Stop();
					tcpListener = null;
				}
			}
		}

        public void TriggerMessageReceived(MailMessageWrapper mmw)
        {
            if (OnMessageReceived != null)
            {
                OnMessageReceived(mmw);
            }
        }

        public void TriggerClientConnected()
        {
            Console.WriteLine("socket open.");
        }

        public void TriggerClientDisconnected()
        {
            Console.WriteLine("socket close.");
        }


		/// <summary>
		/// Forces the server to stop after processing the current request.
		/// </summary>
		public virtual void Stop()
		{
			lock(this)
			{
				// Mark us closed
				stopped = true;
				try 
				{
					// Kick the server accept loop
					tcpListener.Stop();
				} 
				catch
				{
					// Ignore
				}
			}
		}

		/// <overloads>
		/// Creates an instance of SimpleSmtpServer and starts it.
		///	</overloads>
		/// <summary>
		/// Creates and starts an instance of SimpleSmtpServer that will listen on the default port.
		/// </summary>
		/// <returns>The <see cref="SimpleSmtpServer">SmtpServer</see> waiting for message</returns>
		public static SimpleSmtpServer Start()
		{
			return Start(DEFAULT_SMTP_HOST_IP, DEFAULT_SMTP_PORT);
		}

		/// <summary>
		/// Creates and starts an instance of SimpleSmtpServer that will listen on a specific port.
		/// </summary>
		/// <param name="port">port number the server should listen to</param>
		/// <returns>The <see cref="SimpleSmtpServer">SmtpServer</see> waiting for message</returns>
		public static SimpleSmtpServer Start(string hostIp, int port)
		{
			SimpleSmtpServer server = new SimpleSmtpServer(hostIp, port);

			Thread smtpServerThread = new Thread(new ThreadStart(server.Run));
			smtpServerThread.Start();

			// Block until the server socket is created
			try 
			{
				server.startedEvent.WaitOne();
			} 
			catch 
			{
				// Ignore don't care.
			}

			// If an exception occured during server startup, send it back.
			if (server.mainThreadException != null)
				throw server.mainThreadException;

			return server;
		}

	    public void Dispose()
	    {
	        if (stopped == false)
	        {
	            Stop();
	        }
	    }
	}
}
