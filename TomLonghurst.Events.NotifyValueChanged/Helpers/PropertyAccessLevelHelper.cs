using TomLonghurst.Events.NotifyValueChanged.SourceGeneration.Options;
using TomLonghurst.Events.NotifyValueChanged.Wrappers;

namespace TomLonghurst.Events.NotifyValueChanged.Helpers;

internal static class PropertyAccessLevelHelper
{
    public static PropertyAccessLevelMetadata GetPropertyAccessLevelMetadata(NotifyValueChangeAttributeData? attributeData)
    {
        var propertyAccessLevelMetadata = new PropertyAccessLevelMetadata
        {
            MainProperty = "public ",
            Getter = "",
            Setter = ""
        };

        if (attributeData is null)
        {
            return propertyAccessLevelMetadata;
        }

        var getter = attributeData.GetterAccessLevel;
        var setter = attributeData.SetterAccessLevel;

        if (getter >= setter)
        {
            propertyAccessLevelMetadata.MainProperty = MapToString(getter);
            if (setter != getter)
            {
                propertyAccessLevelMetadata.Setter = MapToString(setter);
            }
        }
        else
        {
            propertyAccessLevelMetadata.MainProperty = MapToString(setter);
            propertyAccessLevelMetadata.Getter = MapToString(getter);
        }

        return propertyAccessLevelMetadata;
    }

    private static string MapToString(PropertyAccessLevel propertyAccessLevel)
    {
        return propertyAccessLevel switch
        {
            PropertyAccessLevel.Private => "private ",
            PropertyAccessLevel.PrivateProtected => "private protected ",
            PropertyAccessLevel.Protected => "protected ",
            PropertyAccessLevel.Internal => "internal ",
            PropertyAccessLevel.ProtectedInternal => "protected internal ",
            PropertyAccessLevel.Public => "public ",
            _ => throw new ArgumentOutOfRangeException(nameof(propertyAccessLevel), propertyAccessLevel, null)
        };
    }
}

internal class PropertyAccessLevelMetadata
{
    public string MainProperty { get; set; }
    public string Getter { get; set; }
    public string Setter { get; set; }
}