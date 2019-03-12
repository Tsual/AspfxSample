# AspfxSample
技术样例工程 Aspcore+react

数据库：SQLITE\MYSQL，pooled，事务捆绑

缓存：Redis,静态host，向di注入工厂类

消息队列:RabbitMQ，向di注入工厂类，包装rpc

服务发现:Consul,自动注册，应用终止自动注销，向di容器内添加caller，完成了本地缓存\dns\consul-template to nginx 的负载均衡

ORM:EFcore,完整的标准业务模型案例

Server:Kestral (you can host in iis)

部署方案:1.Nginx+Ubuntu+bash 2.Docker+k8s(未验证,用起来真爽)


Features:

开发模式本地日志,log4n格式,注入在ILogger内


