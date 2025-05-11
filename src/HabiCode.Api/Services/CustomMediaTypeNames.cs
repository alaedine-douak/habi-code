namespace HabiCode.Api.Services;

public static class CustomMediaTypeNames
{
    public static class Application
    {
        public const string HateoasSubType = "hateoas";

        public const string JsonV1 = "application/json;v=1";
        public const string JsonV2 = "application/json;v=2";
        public const string HateoasJson = "application/vnd.habicode.hateoas+json";
        public const string HateoasJsonV1 = "application/vnd.habicode.hateoas.v1+json";
        public const string HateoasJsonV2 = "application/vnd.habicode.hateoas.v2+json";
    }
}
