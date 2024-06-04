# HUB API Client Generation
> see https://aka.ms/autorest


## Getting Started 

To build the SDKs for Veritas API, simply install AutoRest via `npm` (`npm install -g autorest`) and then run:
> `autorest hub-client-autorest-config.md --csharp`

To see additional help and options, run:
> `autorest --help`

For other options on installation see [Installing AutoRest](https://aka.ms/autorest/install) on the AutoRest github page.

For more info about configuration file see [Autorest documentation for configuration files](https://github.com/Azure/autorest/blob/master/docs/user/literate-file-formats/configuration.md) and [Autorest CLI documentation](https://github.com/Azure/autorest/blob/master/docs/user/cli.md).

---

## Configuration 
The following are the settings for this using this API with AutoRest.

``` yaml
# Input swagger file for code generation. Url Local: http://marketplace-tst-mercadolivre.Yahhub.com.br/swagger/v1/swagger.json
input: http://marketplace-tst-mercadolivre.Yahhub.com.br/swagger/v1/swagger.json

# Type of input file.
modeler: swagger

# Output folder for code generation.
output-folder: ./CrossCutting/Yah.Hub.Application/Broker/BrokerServiceApi

# Namespace to be used for generated code.
namespace: Yah.Hub.Application.Broker.BrokerServiceApi

# Name to be used for generated client.
override-client-name: HubMarketplaceClient

# Uncomment to make code generation output to a single file.
output-file: HubMarketplaceClient.cs

```
