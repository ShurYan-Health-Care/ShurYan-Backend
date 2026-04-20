using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Settings;

namespace Shuryan.Application.Services
{
    public class AgoraTokenService : IAgoraTokenService
    {
        private readonly AgoraSettings _settings;

        public AgoraTokenService(IOptions<AgoraSettings> settings)
        {
            _settings = settings.Value;
        }

        public string GenerateRtcToken(string channelName, string uid, bool isPublisher)
        {
            var expire = (uint)_settings.TokenExpirySeconds;
            var token = new AccessToken2(_settings.AppId, _settings.AppCertificate, expire);

            var service = new ServiceRtc(channelName, uid);
            service.AddPrivilege(ServiceRtc.PrivilegeJoinChannel, expire);

            if (isPublisher)
            {
                service.AddPrivilege(ServiceRtc.PrivilegePublishAudioStream, expire);
                service.AddPrivilege(ServiceRtc.PrivilegePublishVideoStream, expire);
                service.AddPrivilege(ServiceRtc.PrivilegePublishDataStream, expire);
            }

            token.AddService(service);
            return token.Build();
        }

        #region AccessToken2 implementation (ported from Agora official C# reference)

        private sealed class AccessToken2
        {
            private const string Version = "007";

            private readonly string _appId;
            private readonly string _appCertificate;
            private readonly uint _expire;
            private readonly uint _issueTs;
            private readonly uint _salt;
            private readonly Dictionary<ushort, ServiceRtc> _services = new();

            public AccessToken2(string appId, string appCertificate, uint expire)
            {
                _appId = appId;
                _appCertificate = appCertificate;
                _expire = expire;
                _issueTs = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                _salt = (uint)Random.Shared.Next(1, int.MaxValue);
            }

            public void AddService(ServiceRtc service) => _services[ServiceRtc.TypeId] = service;

            public string Build()
            {
                // 1. Pack signing buffer: appId + issueTs + expire + salt + services
                var buf = new ByteBuf();
                buf.PutBytes(Encoding.UTF8.GetBytes(_appId));
                buf.PutUint32(_issueTs);
                buf.PutUint32(_expire);
                buf.PutUint32(_salt);
                buf.PutUint16((ushort)_services.Count);
                foreach (var svc in _services.Values)
                    svc.Pack(buf);

                // 2. Derive signing key via HMAC chain
                var signingKey = GetSign();

                // 3. Sign the packed buffer
                using var hmac = new HMACSHA256(signingKey);
                var signature = hmac.ComputeHash(buf.AsBytes());

                // 4. Build content: length-prefixed signature + raw buffer copy
                var content = new ByteBuf();
                content.PutBytes(signature);
                content.PutRaw(buf.AsBytes());

                // 5. Token = "007" + base64(zlib(content))
                return Version + Convert.ToBase64String(Compress(content.AsBytes()));
            }

            private byte[] GetSign()
            {
                using var hmac1 = new HMACSHA256(BitConverter.GetBytes(_issueTs));
                var step1 = hmac1.ComputeHash(Encoding.UTF8.GetBytes(_appCertificate));

                using var hmac2 = new HMACSHA256(BitConverter.GetBytes(_salt));
                return hmac2.ComputeHash(step1);
            }

            private static byte[] Compress(byte[] data)
            {
                using var output = new MemoryStream();
                using (var zlib = new ZLibStream(output, CompressionMode.Compress, leaveOpen: true))
                    zlib.Write(data, 0, data.Length);
                return output.ToArray();
            }
        }

        private sealed class ServiceRtc
        {
            public const ushort TypeId = 1;
            public const ushort PrivilegeJoinChannel = 1;
            public const ushort PrivilegePublishAudioStream = 2;
            public const ushort PrivilegePublishVideoStream = 3;
            public const ushort PrivilegePublishDataStream = 4;

            private readonly string _channelName;
            private readonly string _uid;
            private readonly SortedDictionary<ushort, uint> _privileges = new();

            public ServiceRtc(string channelName, string uid)
            {
                _channelName = channelName;
                _uid = uid;
            }

            public void AddPrivilege(ushort privilege, uint expire) =>
                _privileges[privilege] = expire;

            public void Pack(ByteBuf buf)
            {
                buf.PutUint16(TypeId);
                buf.PutTreeMapUInt32(_privileges);
                buf.PutString(_channelName);
                buf.PutString(_uid);
            }
        }

        private sealed class ByteBuf
        {
            private readonly List<byte> _buf = new();

            public ByteBuf PutUint16(ushort v)
            {
                _buf.Add((byte)(v & 0xFF));
                _buf.Add((byte)((v >> 8) & 0xFF));
                return this;
            }

            public ByteBuf PutUint32(uint v)
            {
                _buf.Add((byte)(v & 0xFF));
                _buf.Add((byte)((v >> 8) & 0xFF));
                _buf.Add((byte)((v >> 16) & 0xFF));
                _buf.Add((byte)((v >> 24) & 0xFF));
                return this;
            }

            public ByteBuf PutBytes(byte[] b)
            {
                PutUint16((ushort)b.Length);
                _buf.AddRange(b);
                return this;
            }

            public ByteBuf PutRaw(byte[] b)
            {
                _buf.AddRange(b);
                return this;
            }

            public ByteBuf PutString(string s) => PutBytes(Encoding.UTF8.GetBytes(s));

            public ByteBuf PutTreeMapUInt32(SortedDictionary<ushort, uint> map)
            {
                PutUint16((ushort)map.Count);
                foreach (var (k, v) in map)
                {
                    PutUint16(k);
                    PutUint32(v);
                }
                return this;
            }

            public byte[] AsBytes() => _buf.ToArray();
        }

        #endregion
    }
}
