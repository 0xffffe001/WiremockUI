﻿using System;
using System.Collections.Generic;
using System.IO;
using WiremockUI.Data;

namespace WiremockUI
{
    public class Dashboard
    {
        public Dictionary<Guid, WireMockServer> Services { get; private set; }
        public Dictionary<Guid, FileSystemWatcher> Watchers { get; private set; }

        public Dashboard()
        {
            this.Services = new Dictionary<Guid, WireMockServer>();
            this.Watchers = new Dictionary<Guid, FileSystemWatcher>();
        }

        public bool IsAnyRunning()
        {
            foreach (var s in Services)
                if (s.Value.IsRunning())
                    return true;

            return false;
        }

        public bool IsRunning(Mock mockService)
        {
            if (mockService == null)
                return false;

            if (!Services.ContainsKey(mockService.Id))
                return false;
            return Services[mockService.Id].IsRunning();
        }

        public void Stop(Mock mockService)
        {
            if (!Services.ContainsKey(mockService.Id))
                return;
            Services[mockService.Id].Stop();
            Services[mockService.Id].ShutDown();
            Services.Remove(mockService.Id);

            if (Watchers.ContainsKey(mockService.Id))
            {
                Watchers[mockService.Id].EnableRaisingEvents = false;
                Watchers.Remove(mockService.Id);
            }
        }

        public void Play(Proxy proxy, Mock mock, bool record, TextWriter textWriter)
        {
            if (Services.ContainsKey(mock.Id))
                Stop(mock);

            var server = new WireMockServer(textWriter);
            Services.Add(mock.Id, server);

            string[] args;

            var relativeFolder = Path.Combine(proxy.GetFolderName(), mock.GetFolderName());
            if (record)
            {
                args = new string[]
                {
                "--port", proxy.PortProxy.ToString(),
                "--proxy-all", proxy.UrlOriginal,
                "--record-mappings",
                "--root-dir", relativeFolder,
                "--verbose"
                };
            }
            else
            {
                args = new string[]
                {
                    "--port", proxy.PortProxy.ToString(),
                    "--root-dir", relativeFolder,
                    "--verbose"
                };
            }

            server.run(args);
        }

        internal void AddWatchers(Mock service, FileSystemWatcher watcher)
        {
            if (Watchers.ContainsKey(service.Id))
                Watchers.Remove(service.Id);
            Watchers.Add(service.Id, watcher);
        }
    }
}