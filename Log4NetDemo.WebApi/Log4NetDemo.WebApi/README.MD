## Log4Net.Demo项目，主要记录了 asp.net core 如何使用log4net的一些用法；

**1：引入包**
首先使用Nuget引入log4net包，版本应>= 2.0.7 

**2：实现 ILogger 接口**

~~~
 public class Log4NetLogger : ILogger
    {
        private readonly ILog _log;
        private ILoggerRepository _loggerRepository;
        public Log4NetLogger(string name, XmlElement xmlElement)
        {
            _loggerRepository = log4net.LogManager.CreateRepository(
                Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));
            _log = LogManager.GetLogger(_loggerRepository.Name, name);
            log4net.Config.XmlConfigurator.Configure(_loggerRepository, xmlElement);
        }
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Critical:
                    return _log.IsFatalEnabled;
                case LogLevel.Debug:
                case LogLevel.Trace:
                    return _log.IsDebugEnabled;
                case LogLevel.Error:
                    return _log.IsErrorEnabled;
                case LogLevel.Information:
                    return _log.IsInfoEnabled;
                case LogLevel.Warning:
                    return _log.IsWarnEnabled;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }
            string message = null;
            if (null != formatter)
            {
                message = formatter(state, exception);
            }
            if (!string.IsNullOrEmpty(message) || exception != null)
            {
                switch (logLevel)
                {
                    case LogLevel.Critical:
                        _log.Fatal(message);
                        break;
                    case LogLevel.Debug:
                    case LogLevel.Trace:
                        _log.Debug(message);
                        break;
                    case LogLevel.Error:
                        _log.Error(message);
                        break;
                    case LogLevel.Information:
                        _log.Info(message);
                        break;
                    case LogLevel.Warning:
                        _log.Warn(message);
                        break;
                    default:
                        _log.Warn($"Encountered unknown log level {logLevel}, writing out as Info.");
                        _log.Info(message, exception);
                        break;
                }
            }
        }
    }
~~~

**3：实现 ILoggerProvider 接口**
~~~
 public class Log4NetProvider : ILoggerProvider
    {
        private readonly string _log4NetConfigFile;
        private readonly ConcurrentDictionary<string, Log4NetLogger> _loggers =
            new ConcurrentDictionary<string, Log4NetLogger>();
        public Log4NetProvider(string log4NetConfigFile)
        {
            _log4NetConfigFile = log4NetConfigFile;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, CreateLoggerImplementation);
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
        private Log4NetLogger CreateLoggerImplementation(string name)
        {
            return new Log4NetLogger(name, Parselog4NetConfigFile(_log4NetConfigFile));
        }

        private static XmlElement Parselog4NetConfigFile(string filename)
        {
            XmlDocument log4netConfig = new XmlDocument();
            log4netConfig.Load(File.OpenRead(filename));
            return log4netConfig["log4net"];
        }
    }
~~~

**4：实现扩展接口**
~~~
 public static class Log4netExtensions
    {
        public static ILoggerFactory AddLog4Net(this ILoggerFactory factory, string log4NetConfigFile)
        {
            factory.AddProvider(new Log4NetProvider(log4NetConfigFile));
            return factory;
        }

        public static ILoggerFactory AddLog4Net(this ILoggerFactory factory)
        {
            factory.AddProvider(new Log4NetProvider("log4net.config"));
            return factory;
        }
    }
~~~

**在Startup加入log4net**

~~~
 public void Configure(IApplicationBuilder app, IHostingEnvironment env,ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            loggerFactory.AddLog4Net();//log4net
            app.UseMvc();
        }
~~~

**log4net.config 实现。可以根据日志级别进行自定义**
~~~
<?xml version="1.0" encoding="utf-8" ?>
<!--LOG4日志级别 
    0：TRACE；记录一些对程序员调试问题有帮助的信息, 其中可能包含一些敏感信息, 所以应该避免在生产环境中启用Trace日志。
    1：DEBUG；记录一些在开发和调试阶段有用的短时变量(Short-term usefulness), 所以除非为了临时排除生产环境的故障，开发人员应该尽量避免在生产环境中启用Debug日志。
    2：INFO；信息日志，记录应用程序的一些流程, 例如，记录当前api请求的url，请求参数等。
    3：WARN；警告日志；记录应用程序中发生的不正常或者未预期的事件信息。这些信息中可能包含错误消息或者错误产生的条件, 例如, 文件未找到，用户不存在。
    4：ERROR；错误日志；记录应用程序中某个操作产生的错误和异常信息，如对空值进行操作等。
    5：FATAL；毁灭性错误；记录一些需要立刻修复的问题。例如数据丢失，磁盘空间不足。
trace<debug<info<warn<error<fatal -->
<log4net>
  <appender name="ErrorRollingFileAppender" type="log4net.Appender.RollingFileAppender">
    <file value="D://log//demo//" />
    <appendToFile value="true" />
    <rollingStyle value="Date"/>
    <datePattern value="yyyy-MM-dd-'error.log'"/>
    <maxSizeRollBackups value="100" />
    <staticLogFileName value="false" />
    <encoding value="utf-8" />
    <layout type="log4net.Layout.PatternLayout">
      <conversionPattern value="%newline%date [%thread %-5level] %n -- %m%n" />
    </layout>
    <filter type="log4net.Filter.LevelRangeFilter">
      <levelMin value="ERROR" />
      <levelMax value="FATAL" />
    </filter>
  </appender>

  <appender name="WarnRollingFileAppender" type="log4net.Appender.RollingFileAppender">
    <file value="D://log//demo//" />
    <appendToFile value="true" />
    <rollingStyle value="Date"/>
    <datePattern value="yyyy-MM-dd-'warn.log'"/>
    <maxSizeRollBackups value="100" />
    <staticLogFileName value="false" />
    <encoding value="utf-8" />
    <layout type="log4net.Layout.PatternLayout">
      <conversionPattern value="%newline%date [%thread %-5level] %n -- %m%n" />
    </layout>
    <filter type="log4net.Filter.LevelRangeFilter">
      <levelMin value="WARN" />
      <levelMax value="WARN" />
    </filter>
  </appender>

  <appender name="InfoRollingFileAppender" type="log4net.Appender.RollingFileAppender">
    <file value="D://log//demo//" />
    <appendToFile value="true" />
    <rollingStyle value="Date"/>
    <datePattern value="yyyy-MM-dd-'info.log'"/>
    <maxSizeRollBackups value="100" />
    <staticLogFileName value="false" />
    <encoding value="utf-8" />
    <layout type="log4net.Layout.PatternLayout">
      <conversionPattern value="%newline%date [%thread %-5level] %n -- %m%n" />
    </layout>
    <filter type="log4net.Filter.LevelRangeFilter">
      <levelMin value="TRACE " />
      <levelMax value="INFO" />
    </filter>
  </appender>

  <root>
    <level value="All" />
    <appender-ref ref="ErrorRollingFileAppender" />
    <appender-ref ref="WarnRollingFileAppender" />
    <appender-ref ref="InfoRollingFileAppender" />
  </root>
</log4net>
~~~

**使用日志**
~~~
 public class ValuesController : ControllerBase
    {
        private readonly ILogger _logger; //注入日志
        public ValuesController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ValuesController>();
        }
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            var result = new string[] { "value1", "value2" };
            //使用日志
            _logger.LogDebug($"返回信息：{string.Join(",",result)}");
            return result;
        }
     }
~~~
