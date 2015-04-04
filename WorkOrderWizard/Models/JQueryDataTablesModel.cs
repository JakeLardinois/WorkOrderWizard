using System.Collections.Generic;
using System.Collections.ObjectModel;

using System.IO;
using System.Web.Script.Serialization;
using System.Text;
using System.Web;


namespace WorkOrderWizard.Models
{
    /// <summary>
    /// Represents the jQuery DataTables request that is sent for server
    /// side processing.
    /// <para>http://datatables.net/usage/server-side</para>
    /// </summary>
    public class JQueryDataTablesModel
    {
        /*I commented out this method, but left it here for reference. The proper way to handle this is to create a custom model binder. JQueryDataTablesModelBinder is such a class and I simply needed to add it to the 
         * model binders collection by adding to the ModelBinders collection in Global.asax.
         */
        /*Modified by Jake Lardinois 7/14/2014; I added this static method to support the scenario where this model needs to be populated by posted raw json data. The MVC linker will take posted data and automatically
         * populate this object, however, when using WCF's linker for json, the embedded collections (bSortable_, bSearchable_, sSearch_, etc..) don't get populated due to how Datatables formats the posted data 
         * (bSortable_0, bSortable_1, bSortable_2, etc. OR bSearchable_0, bSearchable_1, bSearchable_2, etc. OR sSearch_0, sSearch_1, sSearch_2, etc. Respectively). Furthermore, WCF only accepts posted data in the format
         * of xml or json and so the data posted by datatables gets considered invalid unless I explicitly set the 'Content-Type' http header to 'application/json' and then utilize a JQueryDataTablesModel parameter on my
         * on my WCF Interface Parameter. So the only way to build these collections from the posted data was to create this constructor which takes the stream of posted json data and manually populates the collections. 
        */
        //public static JQueryDataTablesModel CreateFromPostData(Stream strmPostData)
        //{
        //    JQueryDataTablesModel objJQueryDataTablesModel;
        //    List<bool> bSortableList, bSearchableList, bRegexList;
        //    List<string> sSearchList, mDataPropList, sSortDirList;
        //    List<int> iSortColList;
        //    JavaScriptSerializer serializer;
        //    string strPostedData;


        //    objJQueryDataTablesModel = new JQueryDataTablesModel();
        //    serializer = new JavaScriptSerializer();
        //    strPostedData = new StreamReader(strmPostData).ReadToEnd();

        //    /*PHP's json_encode sent json to the WCF Web Service like this: {\"sEcho\":\"1\",\"iColumns\":\"17\",\"sColumns\":\",,,,,,,,,,,,,,,,\",...
        //     *However, Javascripts JSON.Stringify sent json to MVC like this: {[{"name":"sEcho","value":1},{"name":"iColumns","value":4},{"name":"sColumns","value":",,,"},...
        //     *So the resolution was to not use JSON.Stringify, but then I recieved json like this: sEcho=1&iColumns=4&sColumns=,,,&
        //     *In order to be parsible I had to perform the below string manipulations
        //     *http://jsonformatter.curiousconcept.com/ helped me to determine valid json
        //     */
        //    strPostedData = HttpUtility.UrlDecode(strPostedData); //This decodes the string which is recieved as encoded; the main culprit were commas which were encoded as %2C
        //    StringBuilder objStrBldr = new StringBuilder(strPostedData
        //        .Replace("=", "\":\"") //replace all the equals with colons and insert quotes per the json standard
        //        .Replace("&", "\",\"") //replace all the ampersands with commas and insert quotes per the json standard
        //        .Insert(0, "{\""))     //add the opening curly bracket to the front of the string to indicate a json object and starting quote for the first property
        //        .Append("\"}");        //use the stringbuilder append to add the closing curly bracket and close the last value string with quotes


        //    strPostedData = objStrBldr.ToString();

        //    //Populate the model with the values from the posted data that will automatically transfer with the javascript deserializer
        //    objJQueryDataTablesModel = serializer.Deserialize<JQueryDataTablesModel>(strPostedData);
        //    //Create a dictionary object that will hold ALL key value pairs from the posted data by using the javascript deserializer
        //    Dictionary<string, string> PostData = serializer.Deserialize<Dictionary<string, string>>(strPostedData);

        //    //var tmp = PostData.ContainsKey

        //    //instantiate my bool collections
        //    bSortableList = new List<bool>();
        //    bSearchableList = new List<bool>();
        //    bRegexList = new List<bool>();

        //    //Populate each collection with the posted data, but first tests to see if collection variables are passed because not all posts from datatables will contain data for every collection
        //    if (PostData.ContainsKey("bSortable_0")) //Checks if the key is in the posted data...
        //        for (int i = 0; i < objJQueryDataTablesModel.iColumns; i++)
        //            bSortableList.Add(bool.Parse(PostData["bSortable_" + i]));
        //    if (PostData.ContainsKey("bSearchable_0")) //Testing here!!!
        //        for (int i = 0; i < objJQueryDataTablesModel.iColumns; i++)
        //            bSearchableList.Add(bool.Parse(PostData["bSearchable_" + i]));
        //    if (PostData.ContainsKey("bRegex_0"))
        //        for (int i = 0; i < objJQueryDataTablesModel.iColumns; i++)
        //            bRegexList.Add(bool.Parse(PostData["bRegex_" + i]));
        //    //Assign the bool collections to the appropriate readonly collections...
        //    objJQueryDataTablesModel.bSortable_ = new System.Collections.ObjectModel.ReadOnlyCollection<bool>(bSortableList);//14
        //    objJQueryDataTablesModel.bSearchable_ = new System.Collections.ObjectModel.ReadOnlyCollection<bool>(bSearchableList);//14
        //    objJQueryDataTablesModel.bRegex_ = new System.Collections.ObjectModel.ReadOnlyCollection<bool>(bRegexList);//14


        //    sSearchList = new List<string>();
        //    mDataPropList = new List<string>();
        //    if (PostData.ContainsKey("sSearch_0"))
        //        for (int i = 0; i < objJQueryDataTablesModel.iColumns; i++)
        //            sSearchList.Add(PostData["sSearch_" + i]);
        //    if (PostData.ContainsKey("mDataProp_0"))
        //        for (int i = 0; i < objJQueryDataTablesModel.iColumns; i++)
        //            mDataPropList.Add(PostData["mDataProp_" + i]);
        //    objJQueryDataTablesModel.sSearch_ = new System.Collections.ObjectModel.ReadOnlyCollection<string>(sSearchList);//14
        //    objJQueryDataTablesModel.mDataProp_ = new System.Collections.ObjectModel.ReadOnlyCollection<string>(mDataPropList);//14


        //    //Do the same here except that the sorting collections are based on the number of sorting columns that are passed...
        //    iSortColList = new List<int>();
        //    if (PostData.ContainsKey("iSortCol_0"))
        //        for (int i = 0; i < objJQueryDataTablesModel.iSortingCols; i++)
        //            iSortColList.Add(int.Parse(PostData["iSortCol_" + i]));
        //    objJQueryDataTablesModel.iSortCol_ = new System.Collections.ObjectModel.ReadOnlyCollection<int>(iSortColList);//1


        //    sSortDirList = new List<string>();
        //    if (PostData.ContainsKey("sSortDir_0"))
        //        for (int i = 0; i < objJQueryDataTablesModel.iSortingCols; i++)
        //            sSortDirList.Add(PostData["sSortDir_" + i]);
        //    objJQueryDataTablesModel.sSortDir_ = new System.Collections.ObjectModel.ReadOnlyCollection<string>(sSortDirList);//1

        //    //This is for searching when reorderable columns are used. A fixed list of column headers must be sent because the reorderable columns get sent in the array based on thier new order in the UI.
        //    if (PostData.ContainsKey("FixedColumnHeaders"))
        //        objJQueryDataTablesModel.mDataProp2_ = new List<string>(PostData["FixedColumnHeaders"].Split(',')).AsReadOnly();


        //    return objJQueryDataTablesModel;
        //}

        /// <summary>
        /// Gets or sets the information for DataTables to use for rendering.
        /// </summary>
        public int sEcho { get; set; }

        /// <summary>
        /// Gets or sets the display start point.
        /// </summary>
        public int iDisplayStart { get; set; }

        /// <summary>
        /// Gets or sets the number of records to display.
        /// </summary>
        public int iDisplayLength { get; set; }

        /// <summary>
        /// Gets or sets the Global search field.
        /// </summary>
        public string sSearch { get; set; }

        /// <summary>
        /// Gets or sets if the Global search is regex or not.
        /// </summary>
        public bool bRegex { get; set; }

        /// <summary>
        /// Gets or sets the number of columns being display (useful for getting individual column search info).
        /// </summary>
        public int iColumns { get; set; }

        /// <summary>
        /// Gets or sets indicator for if a column is flagged as sortable or not on the client-side.
        /// </summary>
        public ReadOnlyCollection<bool> bSortable_ { get; set; }

        /// <summary>
        /// Gets or sets indicator for if a column is flagged as searchable or not on the client-side.
        /// </summary>
        public ReadOnlyCollection<bool> bSearchable_ { get; set; }

        /// <summary>
        /// Gets or sets individual column filter.
        /// </summary>
        public ReadOnlyCollection<string> sSearch_ { get; set; }

        /// <summary>
        /// Gets or sets if individual column filter is regex or not.
        /// </summary>
        public ReadOnlyCollection<bool> bRegex_ { get; set; }

        /// <summary>
        /// Gets or sets the number of columns to sort on.
        /// </summary>
        public int? iSortingCols { get; set; }

        /// <summary>
        /// Gets or sets column being sorted on (you will need to decode this number for your database).
        /// </summary>
        public ReadOnlyCollection<int> iSortCol_ { get; set; }

        /// <summary>
        /// Gets or sets the direction to be sorted - "desc" or "asc".
        /// </summary>
        public ReadOnlyCollection<string> sSortDir_ { get; set; }

        /// <summary>
        /// Gets or sets the value specified by mDataProp for each column. 
        /// This can be useful for ensuring that the processing of data is independent 
        /// from the order of the columns.
        /// </summary>
        public ReadOnlyCollection<string> mDataProp_ { get; set; }

        //Modified by Jake Lardinois 9/2/2013; added another mDataProp Column for server side multi column search that uses sSelector as well as column reordering.
        public ReadOnlyCollection<string> mDataProp2_ { get; set; }

        public ReadOnlyCollection<SortedColumn> GetSortedColumns()
        {
            if (!iSortingCols.HasValue)
            {
                // Return an empty collection since it's easier to work with when verifying against
                return new ReadOnlyCollection<SortedColumn>(new List<SortedColumn>());
            }

            var sortedColumns = new List<SortedColumn>();
            for (int i = 0; i < iSortingCols.Value; i++)
            {
                sortedColumns.Add(new SortedColumn(mDataProp_[iSortCol_[i]], sSortDir_[i]));
            }

            return sortedColumns.AsReadOnly();
        }
    }

    /// <summary>
    /// Represents a sorted column from DataTables.
    /// </summary>
    public class SortedColumn
    {
        private const string Ascending = "asc";

        public SortedColumn(string propertyName, string sortingDirection)
        {
            PropertyName = propertyName;
            Direction = sortingDirection.Equals(Ascending) ? SortingDirection.Ascending : SortingDirection.Descending;
        }

        /// <summary>
        /// Gets the name of the Property on the class to sort on.
        /// </summary>
        public string PropertyName { get; private set; }

        public SortingDirection Direction { get; private set; }

        public override int GetHashCode()
        {
            var directionHashCode = Direction.GetHashCode();
            return PropertyName != null ? PropertyName.GetHashCode() + directionHashCode : directionHashCode;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (GetType() != obj.GetType())
            {
                return false;
            }

            var other = (SortedColumn)obj;

            if (other.Direction != Direction)
            {
                return false;
            }

            return other.PropertyName == PropertyName;
        }
    }

    /// <summary>
    /// Represents the direction of sorting for a column.
    /// </summary>
    public enum SortingDirection
    {
        Ascending,
        Descending
    }
}