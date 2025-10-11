// Copyright © 2023-present https://github.com/dymproject/purest-admin作者以及贡献者

using System.Globalization;
using System.Text;

using SqlSugar;

using Volo.Abp.DependencyInjection;

namespace PurestAdmin.Zero;
public class AutoFrontend(ISqlSugarClient db) : ISingletonDependency
{
    private readonly ISqlSugarClient _db = db;
    private static readonly string[] sourceArray = ["string", "byte[]"];
    
    public void Initialization()
    {
        Console.WriteLine("请选择数据表");
        var tables = _db.DbMaintenance.GetTableInfoList(false);
        for (int i = 0; i < tables.Count; i++)
        {
            Console.WriteLine($"{i}\t{tables[i].Name}");
        }
        var replay = Console.ReadLine() ?? "0";

        var table = tables[int.Parse(replay)];
        Console.WriteLine($"您选择的表是：{table.Name}，请输入类名,如果直接回车则使用默认类名");
        var className = Console.ReadLine();
        if (className.IsNullOrEmpty())
        {
            var nameList = table.Name.Split('_').ToList();
            if (nameList.Count > 1)
            {
                nameList.RemoveAt(0);
            }
            TextInfo ti = new CultureInfo("en-US", false).TextInfo;
            className = nameList.Aggregate("", (current, fName) => current + ti.ToTitleCase(fName.ToLower()));
        }
        Console.WriteLine($"您的类名为：{className}");
        CreateFrontend(table, className);
    }

    public void CreateFrontend(DbTableInfo table, string className)
    {
        var columns = _db.DbMaintenance.GetColumnInfosByTableName(table.Name, false);
        
        // Generate API file
        GenerateApiFile(table.Name, className, columns);
        
        // Generate Vue components
        GenerateVueComponents(table.Name, className, columns);
        
        Console.WriteLine($"Frontend代码生成成功");
    }

    private void GenerateApiFile(string tableName, string className, List<DbColumnInfo> columns)
    {
        // Get the project root directory
        var projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", ".."));
        var apiPath = Path.Combine(projectRoot, "client-vue", "pure-admin", "src", "api", "system");
        Directory.CreateDirectory(apiPath);
        
        var apiFileName = $"{ToKebabCase(className)}.ts";
        var apiFilePath = Path.Combine(apiPath, apiFileName);
        
        var apiContent = GenerateApiContent(tableName, className, columns);
        
        using StreamWriter writer = new(apiFilePath);
        writer.Write(apiContent);
        
        Console.WriteLine($"生成API文件: {apiFilePath}");
    }

    private string GenerateApiContent(string tableName, string className, List<DbColumnInfo> columns)
    {
        var kebabClassName = ToKebabCase(className);
        // Use the actual table name for API endpoints
        var apiEndpointName = ToKebabCase(tableName.Replace("purest_", ""));
        var sb = new StringBuilder();
        
        sb.AppendLine("import { http } from \"@/utils/http\";");
        sb.AppendLine();
        
        // Get all records
        sb.AppendLine($"// Get all {kebabClassName} records");
        sb.AppendLine($"export const getAllList = (params?: any) => {{");
        sb.AppendLine($"  return http.request(\"get\", \"/{apiEndpointName}/{apiEndpointName}s\", params);");
        sb.AppendLine($"}};");
        sb.AppendLine();
        
        // Get paginated list
        sb.AppendLine($"// Get paginated list of {kebabClassName} records");
        sb.AppendLine($"export function getPageList(params?: any) {{");
        sb.AppendLine($"  return http.request(\"get\", \"/{apiEndpointName}/paged-list\", {{ params }});");
        sb.AppendLine($"}};");
        sb.AppendLine();
        
        // Submit data (create or update)
        sb.AppendLine($"// Submit data (create or update)");
        sb.AppendLine($"export const submitData = (params: any) => {{");
        sb.AppendLine($"  return http.request(");
        sb.AppendLine($"    params.id ? \"put\" : \"post\",");
        sb.AppendLine($"    `/{apiEndpointName}/${{params.id ?? \"\"}}`,");
        sb.AppendLine($"    {{");
        sb.AppendLine($"      data: params");
        sb.AppendLine($"    }}");
        sb.AppendLine($"  );");
        sb.AppendLine($"}};");
        sb.AppendLine();
        
        // Get single record
        sb.AppendLine($"// Get a single {kebabClassName} record by ID");
        sb.AppendLine($"export const getSingle = (id: number) => {{");
        sb.AppendLine($"  return http.request(\"get\", `/{apiEndpointName}/${{id}}`);");
        sb.AppendLine($"}};");
        sb.AppendLine();
        
        // Delete record
        sb.AppendLine($"// Delete a {kebabClassName} record by ID");
        sb.AppendLine($"export const deleteData = (id: number) => {{");
        sb.AppendLine($"  return http.request(\"delete\", `/{apiEndpointName}/${{id}}`);");
        sb.AppendLine($"}};");
        sb.AppendLine();
        
        // Batch delete
        sb.AppendLine($"// Batch delete {kebabClassName} records");
        sb.AppendLine($"export const batchDelete = (ids: number[]) => {{");
        sb.AppendLine($"  return http.request(\"delete\", \"/{apiEndpointName}/batch\", {{");
        sb.AppendLine($"    data: {{ ids }}");
        sb.AppendLine($"  }});");
        sb.AppendLine($"}};");
        sb.AppendLine();
        
        // Export
        sb.AppendLine($"// Export {kebabClassName} records");
        sb.AppendLine($"export const exportData = (params?: any) => {{");
        sb.AppendLine($"  return http.request(\"get\", \"/{apiEndpointName}/export\", {{");
        sb.AppendLine($"    params,");
        sb.AppendLine($"    responseType: \"blob\"");
        sb.AppendLine($"  }});");
        sb.AppendLine($"}};");
        
        return sb.ToString();
    }

    private void GenerateVueComponents(string tableName, string className, List<DbColumnInfo> columns)
    {
        // Get the project root directory
        var projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", ".."));
        var vuePath = Path.Combine(projectRoot, "client-vue", "pure-admin", "src", "views", "system", ToKebabCase(className));
        Directory.CreateDirectory(vuePath);
        
        // Generate index.vue
        var indexVuePath = Path.Combine(vuePath, "index.vue");
        var indexVueContent = GenerateIndexVueContent(tableName, className, columns);
        using StreamWriter indexWriter = new(indexVuePath);
        indexWriter.Write(indexVueContent);
        Console.WriteLine($"生成Vue主文件: {indexVuePath}");
        
        // Generate CreateModal.vue
        var createModalVuePath = Path.Combine(vuePath, "CreateModal.vue");
        var createModalVueContent = GenerateCreateModalVueContent(tableName, className, columns);
        using StreamWriter createModalWriter = new(createModalVuePath);
        createModalWriter.Write(createModalVueContent);
        Console.WriteLine($"生成Vue模态框文件: {createModalVuePath}");
    }

    private string GenerateIndexVueContent(string tableName, string className, List<DbColumnInfo> columns)
    {
        var kebabClassName = ToKebabCase(className);
        // Use the actual table name for API endpoints
        var apiEndpointName = ToKebabCase(tableName.Replace("purest_", ""));
        var sb = new StringBuilder();
        
        sb.AppendLine("<script lang=\"ts\" setup>");
        sb.AppendLine("import { h, reactive, ref } from \"vue\";");
        sb.AppendLine("import { getPageList, deleteData } from \"@/api/system/" + kebabClassName + "\";");
        sb.AppendLine("import { ReVxeGrid } from \"@/components/ReVxeTable\";");
        sb.AppendLine("import { VxeButton, VxeUI } from \"vxe-pc-ui\";");
        sb.AppendLine("import CreateModal from \"./CreateModal.vue\";");
        sb.AppendLine();
        sb.AppendLine("const reVxeGridRef = ref();");
        sb.AppendLine("const columns = [");
        sb.AppendLine("  { type: \"checkbox\", title: \"\", width: 60, align: \"center\" },");
        
        // Generate columns based on database columns
        foreach (var column in columns)
        {
            // Skip base columns
            string[] baseColumnName = ["ID", "CREATE_BY", "CREATE_TIME", "UPDATE_BY", "UPDATE_TIME"];
            if (baseColumnName.Contains(column.DbColumnName.ToUpper()))
            {
                continue;
            }
            
            var fieldName = ToCamelCase(column.DbColumnName);
            var title = !string.IsNullOrEmpty(column.ColumnDescription) ? column.ColumnDescription : ToPascalCase(column.DbColumnName);
            
            // Handle special columns
            if (column.DbColumnName.ToUpper() == "PARENT_ID")
            {
                sb.AppendLine("  {");
                sb.AppendLine($"    title: \"{title}\",");
                sb.AppendLine($"    field: \"{fieldName}\",");
                sb.AppendLine("    treeNode: true,");
                sb.AppendLine("    minWidth: 100");
                sb.AppendLine("  },");
            }
            else
            {
                sb.AppendLine("  {");
                sb.AppendLine($"    title: \"{title}\",");
                sb.AppendLine($"    field: \"{fieldName}\",");
                sb.AppendLine("    minWidth: 150");
                
                // Add formatter for date fields
                if (column.DataType.ToLower().Contains("date") || column.DataType.ToLower().Contains("time"))
                {
                    sb.AppendLine("    ,formatter: ({ cellValue }) => {");
                    sb.AppendLine("      return cellValue ? new Date(cellValue).toLocaleDateString() : \"\";");
                    sb.AppendLine("    }");
                }
                
                sb.AppendLine("  },");
            }
        }
        
        sb.AppendLine("];");
        sb.AppendLine("const formRef = ref();");
        sb.AppendLine();
        sb.AppendLine("const handleInitialFormParams = () => ({");
        
        // Add search form fields (only string fields for search)
        bool firstField = true;
        foreach (var column in columns)
        {
            string[] baseColumnName = ["ID", "CREATE_BY", "CREATE_TIME", "UPDATE_BY", "UPDATE_TIME", "PARENT_ID"];
            if (baseColumnName.Contains(column.DbColumnName.ToUpper()))
            {
                continue;
            }
            
            var fieldName = ToCamelCase(column.DbColumnName);
            var csharpType = GetCsharpType(column);
            
            // Only add string fields to search form
            if (csharpType == "string")
            {
                if (!firstField) sb.Append(",\n");
                sb.Append($"  {fieldName}: \"\"");
                firstField = false;
            }
        }
        
        sb.AppendLine();
        sb.AppendLine("});");
        sb.AppendLine("const formItems = [");
        
        // Generate form items for search
        foreach (var column in columns)
        {
            string[] baseColumnName = ["ID", "CREATE_BY", "CREATE_TIME", "UPDATE_BY", "UPDATE_TIME", "PARENT_ID"];
            if (baseColumnName.Contains(column.DbColumnName.ToUpper()))
            {
                continue;
            }
            
            var fieldName = ToCamelCase(column.DbColumnName);
            var csharpType = GetCsharpType(column);
            var title = !string.IsNullOrEmpty(column.ColumnDescription) ? column.ColumnDescription : ToPascalCase(column.DbColumnName);
            
            // Only add string fields to search form
            if (csharpType == "string")
            {
                sb.AppendLine("  {");
                sb.AppendLine($"    field: \"{fieldName}\",");
                sb.AppendLine($"    title: \"{title}\",");
                sb.AppendLine("    span: 6,");
                sb.AppendLine("    itemRender: { name: \"$input\", props: { placeholder: \"" + title + "\" } }");
                sb.AppendLine("  },");
            }
        }
        
        sb.AppendLine("  {");
        sb.AppendLine("    span: 6,");
        sb.AppendLine("    itemRender: {");
        sb.AppendLine("      name: \"$buttons\",");
        sb.AppendLine("      children: [");
        sb.AppendLine("        {");
        sb.AppendLine("          props: {");
        sb.AppendLine("            type: \"submit\",");
        sb.AppendLine("            icon: \"vxe-icon-search\",");
        sb.AppendLine("            content: \"Search\",");
        sb.AppendLine("            status: \"primary\"");
        sb.AppendLine("          }");
        sb.AppendLine("        },");
        sb.AppendLine("        { props: { type: \"reset\", icon: \"vxe-icon-undo\", content: \"Reset\" } }");
        sb.AppendLine("      ]");
        sb.AppendLine("    }");
        sb.AppendLine("  }");
        sb.AppendLine("];");
        sb.AppendLine("const formData = reactive(handleInitialFormParams());");
        sb.AppendLine();
        sb.AppendLine("const handleSearch = () => {");
        sb.AppendLine("  reVxeGridRef.value.loadData();");
        sb.AppendLine("};");
        sb.AppendLine();
        sb.AppendLine("const createModalRef = ref();");
        sb.AppendLine("const handleAdd = () => {");
        sb.AppendLine("  createModalRef.value.showAddModal();");
        sb.AppendLine("};");
        sb.AppendLine("const handleEdit = (record: Recordable) => {");
        sb.AppendLine("  createModalRef.value.showEditModal(record);");
        sb.AppendLine("};");
        sb.AppendLine("const handleDelete = async (record: Recordable) => {");
        sb.AppendLine("  const type = await VxeUI.modal.confirm(\"Are you sure you want to delete this record?\");");
        sb.AppendLine("  if (type == \"confirm\") {");
        sb.AppendLine("    deleteData(record.id).then(() => {");
        sb.AppendLine("      handleSearch();");
        sb.AppendLine("    });");
        sb.AppendLine("  }");
        sb.AppendLine("};");
        sb.AppendLine("const handleView = (record: Recordable) => {");
        sb.AppendLine("  createModalRef.value.showViewModal(record);");
        sb.AppendLine("};");
        sb.AppendLine("const functions: Record<string, string> = {");
        sb.AppendLine($"  add: \"system.{kebabClassName}.add\",");
        sb.AppendLine($"  edit: \"system.{kebabClassName}.edit\",");
        sb.AppendLine($"  view: \"system.{kebabClassName}.view\",");
        sb.AppendLine($"  delete: \"system.{kebabClassName}.delete\"");
        sb.AppendLine("};");
        sb.AppendLine("</script>");
        sb.AppendLine("<template>");
        sb.AppendLine("  <div>");
        sb.AppendLine("    <el-card :shadow=\"`never`\">");
        sb.AppendLine("      <vxe-form");
        sb.AppendLine("        ref=\"formRef\"");
        sb.AppendLine("        :data=\"formData\"");
        sb.AppendLine("        :items=\"formItems\"");
        sb.AppendLine("        @submit=\"handleSearch\"");
        sb.AppendLine("        @reset=\"handleInitialFormParams\"");
        sb.AppendLine("      />");
        sb.AppendLine("    </el-card>");
        sb.AppendLine("    <el-card :shadow=\"`never`\" class=\"table-card\">");
        sb.AppendLine("      <ReVxeGrid");
        sb.AppendLine("        ref=\"reVxeGridRef\"");
        sb.AppendLine("        :request=\"getPageList\"");
        sb.AppendLine("        :functions=\"functions\"");
        sb.AppendLine("        :searchParams=\"formData\"");
        sb.AppendLine("        :columns=\"columns\"");
        sb.AppendLine("        @handleAdd=\"handleAdd\"");
        sb.AppendLine("        @handleEdit=\"handleEdit\"");
        sb.AppendLine("        @handleDelete=\"handleDelete\"");
        sb.AppendLine("        @handleView=\"handleView\"");
        sb.AppendLine("      />");
        sb.AppendLine("    </el-card>");
        sb.AppendLine("    <CreateModal ref=\"createModalRef\" @reload=\"handleSearch\" />");
        sb.AppendLine("  </div>");
        sb.AppendLine("</template>");
        
        return sb.ToString();
    }

    private string GenerateCreateModalVueContent(string tableName, string className, List<DbColumnInfo> columns)
    {
        var kebabClassName = ToKebabCase(className);
        // Use the actual table name for API endpoints
        var apiEndpointName = ToKebabCase(tableName.Replace("purest_", ""));
        var sb = new StringBuilder();
        
        sb.AppendLine("<script lang=\"ts\" setup>");
        sb.AppendLine("import { ref, nextTick, reactive, h } from \"vue\";");
        sb.AppendLine("import { VxeModalInstance, VxeFormInstance, VxeFormPropTypes } from \"vxe-pc-ui\";");
        sb.AppendLine("import { getSingle, submitData } from \"@/api/system/" + kebabClassName + "\";");
        sb.AppendLine("const emits = defineEmits<{ (e: \"reload\"): void }>();");
        sb.AppendLine("const vxeModalRef = ref<VxeModalInstance>();");
        sb.AppendLine("const modalOptions = reactive<{");
        sb.AppendLine("  modalValue: boolean;");
        sb.AppendLine("  modalTitle: string;");
        sb.AppendLine("  canSubmit: boolean;");
        sb.AppendLine("}>({");
        sb.AppendLine("  modalValue: false,");
        sb.AppendLine("  modalTitle: \"\",");
        sb.AppendLine("  canSubmit: true");
        sb.AppendLine("});");
        sb.AppendLine();
        sb.AppendLine("const showModal = (title: string, canSubmit?: boolean): void => {");
        sb.AppendLine("  modalOptions.modalTitle = title;");
        sb.AppendLine("  modalOptions.modalValue = true;");
        sb.AppendLine("  modalOptions.canSubmit = canSubmit ?? true;");
        sb.AppendLine("};");
        sb.AppendLine();
        sb.AppendLine("interface " + ToPascalCase(className) + "Input {");
        
        // Generate interface properties
        foreach (var column in columns)
        {
            string[] baseColumnName = ["CREATE_BY", "CREATE_TIME", "UPDATE_BY", "UPDATE_TIME"];
            if (baseColumnName.Contains(column.DbColumnName.ToUpper()))
            {
                continue;
            }
            
            var fieldName = ToCamelCase(column.DbColumnName);
            var csharpType = GetCsharpType(column);
            var tsType = GetTypescriptType(csharpType);
            
            sb.AppendLine($"  {fieldName}: {tsType};");
        }
        
        sb.AppendLine("}");
        sb.AppendLine("const formRef = ref<VxeFormInstance>();");
        sb.AppendLine("const defaultFormData = () => {");
        sb.AppendLine("  return {");
        
        // Generate default form data
        bool firstField = true;
        foreach (var column in columns)
        {
            string[] baseColumnName = ["CREATE_BY", "CREATE_TIME", "UPDATE_BY", "UPDATE_TIME"];
            if (baseColumnName.Contains(column.DbColumnName.ToUpper()))
            {
                continue;
            }
            
            var fieldName = ToCamelCase(column.DbColumnName);
            var csharpType = GetCsharpType(column);
            var defaultValue = GetDefaultValue(csharpType);
            
            if (!firstField) sb.Append(",\n");
            sb.Append($"    {fieldName}: {defaultValue}");
            firstField = false;
        }
        
        sb.AppendLine();
        sb.AppendLine("  };");
        sb.AppendLine("};");
        sb.AppendLine("const formData = ref<" + ToPascalCase(className) + "Input>(defaultFormData());");
        sb.AppendLine("const formItems = ref<VxeFormPropTypes.Items>([");
        
        // Generate form items based on actual database columns
        foreach (var column in columns)
        {
            string[] baseColumnName = ["ID", "CREATE_BY", "CREATE_TIME", "UPDATE_BY", "UPDATE_TIME"];
            if (baseColumnName.Contains(column.DbColumnName.ToUpper()))
            {
                continue;
            }
            
            var fieldName = ToCamelCase(column.DbColumnName);
            var csharpType = GetCsharpType(column);
            var title = !string.IsNullOrEmpty(column.ColumnDescription) ? column.ColumnDescription : ToPascalCase(column.DbColumnName);
            
            sb.AppendLine("  {");
            sb.AppendLine($"    field: \"{fieldName}\",");
            sb.AppendLine($"    title: \"{title}\",");
            sb.AppendLine("    span: 24,");
            
            // Set input type based on data type
            string inputType = "$input";
            string inputProps = "placeholder: \"Please enter " + title + "\"";
            
            if (csharpType == "int" || csharpType == "long" || csharpType == "decimal")
            {
                inputProps = "type: \"number\", placeholder: \"Please enter " + title + "\"";
            }
            else if (csharpType == "DateTime")
            {
                inputType = "$input";
                inputProps = "type: \"date\", placeholder: \"Please select " + title + "\"";
            }
            else if (csharpType == "bool")
            {
                inputType = "$radio";
                inputProps = "options: [{ label: 'Yes', value: true }, { label: 'No', value: false }]";
            }
            else if (column.DataType.ToLower().Contains("text"))
            {
                inputType = "$textarea";
                inputProps = "placeholder: \"Please enter " + title + "\"";
            }
            
            sb.AppendLine("    itemRender: {");
            sb.AppendLine($"      name: \"{inputType}\",");
            sb.AppendLine($"      props: {{ {inputProps} }}");
            sb.AppendLine("    }");
            sb.AppendLine("  },");
        }
        
        sb.AppendLine("]);");
        sb.AppendLine("const formRules = ref<VxeFormPropTypes.Rules>({");
        
        // Add validation rules based on actual database columns
        foreach (var column in columns)
        {
            string[] baseColumnName = ["ID", "CREATE_BY", "CREATE_TIME", "UPDATE_BY", "UPDATE_TIME"];
            if (baseColumnName.Contains(column.DbColumnName.ToUpper()))
            {
                continue;
            }
            
            // Add required validation for non-nullable fields
            if (!column.IsNullable)
            {
                var fieldName = ToCamelCase(column.DbColumnName);
                var title = !string.IsNullOrEmpty(column.ColumnDescription) ? column.ColumnDescription : ToPascalCase(column.DbColumnName);
                sb.AppendLine($"  {fieldName}: [{{ required: true, message: \"Please enter {title}\" }}],");
            }
        }
        
        sb.AppendLine("});");
        sb.AppendLine();
        sb.AppendLine("const showAddModal = () => {");
        sb.AppendLine("  showModal(`Add " + ToPascalCase(className) + "`);");
        sb.AppendLine("  formData.value = defaultFormData();");
        sb.AppendLine("  nextTick(() => {");
        sb.AppendLine("    formRef.value.clearValidate();");
        sb.AppendLine("  });");
        sb.AppendLine("};");
        sb.AppendLine("const showEditModal = (record: Recordable) => {");
        sb.AppendLine("  showModal(`Edit " + ToPascalCase(className) + "->${record.name || record.id}`);");
        sb.AppendLine("  nextTick(async () => {");
        sb.AppendLine("    formRef.value.clearValidate();");
        sb.AppendLine("    getSingle(record.id).then((data: any) => {");
        sb.AppendLine("      formData.value = data;");
        sb.AppendLine("    });");
        sb.AppendLine("  });");
        sb.AppendLine("};");
        sb.AppendLine("const showViewModal = (record: Recordable) => {");
        sb.AppendLine("  showModal(`View " + ToPascalCase(className) + "->${record.name || record.id}`, false);");
        sb.AppendLine("  nextTick(async () => {");
        sb.AppendLine("    formRef.value.clearValidate();");
        sb.AppendLine("    getSingle(record.id).then((data: any) => {");
        sb.AppendLine("      formData.value = data;");
        sb.AppendLine("    });");
        sb.AppendLine("  });");
        sb.AppendLine("};");
        sb.AppendLine("const handleSubmit = async () => {");
        sb.AppendLine("  const validate = await formRef.value.validate();");
        sb.AppendLine("  if (!validate) {");
        sb.AppendLine("    submitData(formData.value).then(() => {");
        sb.AppendLine("      modalOptions.modalValue = false;");
        sb.AppendLine("      emits(\"reload\");");
        sb.AppendLine("    });");
        sb.AppendLine("  }");
        sb.AppendLine("};");
        sb.AppendLine();
        sb.AppendLine("defineExpose({ showAddModal, showEditModal, showViewModal });");
        sb.AppendLine("</script>");
        sb.AppendLine("<template>");
        sb.AppendLine("  <vxe-modal");
        sb.AppendLine("    ref=\"vxeModalRef\"");
        sb.AppendLine("    v-model=\"modalOptions.modalValue\"");
        sb.AppendLine("    width=\"600\"");
        sb.AppendLine("    height=\"400\"");
        sb.AppendLine("    showFooter");
        sb.AppendLine("    :destroy-on-close=\"true\"");
        sb.AppendLine("    :title=\"modalOptions.modalTitle\"");
        sb.AppendLine("  >");
        sb.AppendLine("    <template #default>");
        sb.AppendLine("      <vxe-form");
        sb.AppendLine("        ref=\"formRef\"");
        sb.AppendLine("        :data=\"formData\"");
        sb.AppendLine("        :items=\"formItems\"");
        sb.AppendLine("        :rules=\"formRules\"");
        sb.AppendLine("        :titleWidth=\"100\"");
        sb.AppendLine("        :titleColon=\"true\"");
        sb.AppendLine("        :titleAlign=\"`right`\"");
        sb.AppendLine("      />");
        sb.AppendLine("    </template>");
        sb.AppendLine("    <template #footer>");
        sb.AppendLine("      <vxe-button content=\"Close\" @click=\"modalOptions.modalValue = false\" />");
        sb.AppendLine("      <vxe-button");
        sb.AppendLine("        v-if=\"modalOptions.canSubmit\"");
        sb.AppendLine("        status=\"primary\"");
        sb.AppendLine("        content=\"Confirm\"");
        sb.AppendLine("        @click=\"handleSubmit\"");
        sb.AppendLine("      />");
        sb.AppendLine("    </template>");
        sb.AppendLine("  </vxe-modal>");
        sb.AppendLine("</template>");
        
        return sb.ToString();
    }

    private string GetCsharpType(DbColumnInfo columnInfo)
    {
        string strType;
        strType = columnInfo.DataType.ToLower() switch
        {
            "tinyint" => columnInfo.Length == 1 ? "bool" : "int",
            "smallint" or "mediumint" or "int" => "int",
            "bigint" => "long",
            "float" or "double" => "decimal",
            "decimal" => columnInfo.Scale > 0 ? "decimal" : (columnInfo.Length > 10 ? "long" : "int"),
            "char" or "varchar" or "text" or "tinytext" or "mediumtext" or "longtext" => "string",
            "blob" or "tinyblob" or "mediumblob" or "longblob" => "byte[]",
            "date" or "datetime" => "DateTime",
            _ => "string",
        };
        return strType;
    }

    private string GetTypescriptType(string csharpType)
    {
        return csharpType switch
        {
            "bool" => "boolean",
            "int" => "number",
            "long" => "number",
            "decimal" => "number",
            "string" => "string",
            "DateTime" => "string",
            "byte[]" => "any",
            _ => "any",
        };
    }

    private string GetDefaultValue(string csharpType)
    {
        return csharpType switch
        {
            "bool" => "false",
            "int" => "null",
            "long" => "null",
            "decimal" => "null",
            "string" => "\"\"",
            "DateTime" => "null",
            "byte[]" => "null",
            _ => "null",
        };
    }

    private string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        TextInfo ti = new CultureInfo("en-US", false).TextInfo;
        var nameList = input.Split('_').ToList();
        return nameList.Aggregate("", (current, fName) => current + ti.ToTitleCase(fName.ToLower()));
    }

    private string ToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var pascalCase = ToPascalCase(input);
        return char.ToLower(pascalCase[0]) + pascalCase.Substring(1);
    }

    private string ToKebabCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var result = new StringBuilder();
        bool first = true;

        foreach (char c in input)
        {
            if (char.IsUpper(c) && !first)
            {
                result.Append('-');
            }
            result.Append(char.ToLower(c));
            first = false;
        }

        return result.ToString();
    }
}