namespace Andromeda.Framing.Metadata
{
    public abstract class MetadataParserWithId<T> : MetadataParser<T>
        where T : MessageMetadataWithId
    {
    }
}
