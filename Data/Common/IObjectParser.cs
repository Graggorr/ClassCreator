namespace ClassCreator.Data.Common
{
    /// <summary>
    /// A class which parses <see cref="TEntity"/> to <see cref="TDto"/> and opposite.
    /// </summary>
    public interface IObjectParser<TEntity, TDto> where TEntity : IObjectDataEntity where TDto : IObjectDataDto
    {
        /// <summary>
        /// Converts incoming instance of <see cref="TDto"/> to <see cref="TEntity"/>
        /// </summary>
        /// <param name="dto">The chosen dto to be converted</param>
        /// <returns>A new created instance of <see cref="TEntity"/> if validation is success; otherwise - null</returns>
        public TEntity? CreateObjectData(TDto dto);

        /// <summary>
        /// Converts incoming instance of <see cref="TEntity"/> into the <see cref="TDto"/>
        /// </summary>
        /// <param name="objectData">Instance of <see cref="TEntity"/> to be converted</param>
        /// <returns>A new created instance of <see cref="TDto"/></returns>
        public TDto? GetObjectDataDto(TEntity entity);
    }
}
