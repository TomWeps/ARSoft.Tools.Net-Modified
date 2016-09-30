using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using ARSoft.Tools.Net.Dns;
using ARSoft.Tools.Net.Test.Consul.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ARSoft.Tools.Net.Test.Consul
{
    [TestClass]
    public class DnsClientTest
    {
        public TestContext TestContext { get; set; }

        private ConsulRunner consulRunner;
        private const string ConsulResources = @"Resources\";

        private DnsClient dnsClient;

        [TestInitialize]
        public void Init()
        {
            consulRunner = new ConsulRunner(TestContext.TestDeploymentDir);

            var ipAddress = new IPAddress(new byte[] {127, 0, 0, 1});
            var dnsPort = 8600; // Consul dns Port
            var ipEndPoint = new IPEndPoint(ipAddress, dnsPort);
            int queryTimeout = 1000;

            dnsClient = new DnsClient(new[] {ipEndPoint}, queryTimeout);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (consulRunner != null)
            {
                consulRunner.Cleanup();
                consulRunner = null;
            }
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        [DeploymentItem(ConsulResources)]
        public void ShouldResolveByNodeName()
        {
            //Arrange
            var domainName = new DomainName(new[] { "test", "node", "dc1", "consul" });

            //Setup Infrastructure			
            consulRunner.Init("consulConfig_Case01.json");

            //Act			
            var dnsMessage = dnsClient.Resolve(domainName, RecordType.A);

            //Assert
            #region DNS Answer
            /* 
            dig @127.0.0.1 -p 8600 test.node.dc1.consul

            ; <<>> DiG 9.10.4-P2 <<>> @127.0.0.1 -p 8600 test.node.dc1.consul
            ; (1 server found)
            ;; global options: +cmd
            ;; Got answer:
            ;; ->>HEADER<<- opcode: QUERY, status: NOERROR, id: 56663
            ;; flags: qr aa rd; QUERY: 1, ANSWER: 1, AUTHORITY: 0, ADDITIONAL: 0
            ;; WARNING: recursion requested but not available

            ;; QUESTION SECTION:
            ;test.node.dc1.consul.          IN      A

            ;; ANSWER SECTION:
            test.node.dc1.consul.   0       IN      A       127.0.0.1

            ;; Query time: 0 msec
            ;; SERVER: 127.0.0.1#8600(127.0.0.1)
            ;; WHEN: Fri Sep 30 23:29:30 Central European Daylight Time 2016
            ;; MSG SIZE  rcvd: 74
            */
            #endregion DNS Answer

            Assert.IsNotNull(dnsMessage);
            Assert.AreEqual(1, dnsMessage.AnswerRecords.Count);
            ARecord aRecord = dnsMessage.AnswerRecords[0] as ARecord;
            Assert.IsNotNull(aRecord);
            Assert.AreEqual("127.0.0.1",aRecord.Address.ToString());
            Assert.AreEqual("test.node.dc1.consul.",aRecord.Name.ToString());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        [DeploymentItem(ConsulResources)]
        public void ShouldResolveByServiceName()
        {
            //Arrange
            var domainName = new DomainName(new[] {"v1", "ServiceA", "service", "consul"});

            //Setup Infrastructure			
            consulRunner.Init("consulConfig_Case01.json");

            //Act			
            var dnsMessage = dnsClient.Resolve(domainName, RecordType.Srv);

            //Assert
            #region DNS Answer
            /* 
            dig @127.0.0.1 -p 8600 v1.servicea.service.consul SRV

            ; <<>> DiG 9.10.4-P2 <<>> @127.0.0.1 -p 8600 v1.servicea.service.consul SRV
            ; (1 server found)
            ;; global options: +cmd
            ;; Got answer:
            ;; ->>HEADER<<- opcode: QUERY, status: NOERROR, id: 57867
            ;; flags: qr aa rd; QUERY: 1, ANSWER: 2, AUTHORITY: 0, ADDITIONAL: 2
            ;; WARNING: recursion requested but not available

            ;; QUESTION SECTION:
            ;v1.servicea.service.consul.    IN      SRV

            ;; ANSWER SECTION:
            v1.servicea.service.consul. 0   IN      SRV     1 1 1231 test.node.dc1.consul.
            v1.servicea.service.consul. 0   IN      SRV     1 1 1232 test.node.dc1.consul.

            ;; ADDITIONAL SECTION:
            test.node.dc1.consul. 0 IN A     192.168.0.1
            test.node.dc1.consul. 0 IN A     192.168.0.2

            ;; Query time: 0 msec
            ;; SERVER: 127.0.0.1#8600(127.0.0.1)
            ;; WHEN: Fri Sep 30 23:23:58 Central European Daylight Time 2016
            ;; MSG SIZE  rcvd: 308  
            */
            #endregion DNS Answer

            Assert.IsNotNull(dnsMessage);
            Assert.AreEqual(2, dnsMessage.AnswerRecords.Count);
            Assert.AreEqual(2, dnsMessage.AdditionalRecords.Count);

            var srvRecords = dnsMessage.AnswerRecords.Cast<SrvRecord>().ToList();
            var aRecords = dnsMessage.AdditionalRecords.Cast<ARecord>().ToList();

            int indexOfFirstSrv = srvRecords.FindIndex(r => r.Port == 1231 && 
                                                        r.Name.ToString() == "v1.ServiceA.service.consul." && 
                                                        r.Target.ToString() == "test.node.dc1.consul.");
            int indexOfSecondSrv = srvRecords.FindIndex(r => r.Port == 1232 && 
                                                        r.Name.ToString() == "v1.ServiceA.service.consul." && 
                                                        r.Target.ToString() == "test.node.dc1.consul.");
            int indexOfFirstA = aRecords.FindIndex(r => r.Address.ToString() == "192.168.0.1");
            int indexOfSecondA = aRecords.FindIndex(r => r.Address.ToString() == "192.168.0.2");

            Assert.AreNotEqual(-1, indexOfFirstSrv);
            Assert.AreNotEqual(-1, indexOfSecondSrv);
            Assert.AreNotEqual(-1, indexOfFirstA);
            Assert.AreNotEqual(-1, indexOfSecondA);
            Assert.IsTrue(indexOfFirstSrv == indexOfFirstA);
            Assert.IsTrue(indexOfSecondSrv == indexOfSecondA);
        }
    }
}
