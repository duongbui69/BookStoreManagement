$conn = New-Object System.Data.SqlClient.SqlConnection("Server=(localdb)\MSSQLLocalDB;Database=BookStoreDb;Trusted_Connection=True;")
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT Id, AuthorName FROM Authors WHERE AuthorName LIKE '%Ng%'"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Host "$($reader['Id']) - $($reader['AuthorName'])"
}
$conn.Close()
