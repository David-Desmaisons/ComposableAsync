using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyActor
{
    /// <summary>
    ///  Factory to create load balander from POCO
    /// </summary>
    public interface ILoadBalancerFactory
    {
        /// <summary>
        ///  Build asynchroneously an loader balancer from a POCO
        ///  using the actor thread to call the function creating the POCO.
        ///  T should an interface througth which the loader balancer  will be seen
        ///  <param name="ParrallelLimitation">
        ///  maximum number of actor providing functionality
        ///  </param>
        /// </summary>
        T Build<T>(Func<T> concrete, int ParrallelLimitation) where T : class;
    }
}
