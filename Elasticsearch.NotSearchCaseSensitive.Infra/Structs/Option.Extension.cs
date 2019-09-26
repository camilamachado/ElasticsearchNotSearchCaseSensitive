namespace Elasticsearch.NotSearchCaseSensitive.Infra.Structs
{
    /// <summary>
    ///  Extensão da classe Option para implementar métodos úteis na representação um objeto de retorno que pode ser nulo.
    /// </summary>
    public static class Option
    {
        /// <summary>
        ///  Método que retorna uma nova instancia de Option com base no paramêtro value
        /// </summary>
        /// <param name="value">É o resultado da operação</param>
        public static Option<T> Of<T>(T value) => new Option<T>(value, value != null);
    }
}