$conn = New-Object System.Data.SqlClient.SqlConnection("Server=(localdb)\MSSQLLocalDB;Database=BookStoreDb;Trusted_Connection=True;")
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "UPDATE Authors SET Nationality = N'Việt Nam' WHERE AuthorName LIKE N'%Thuần' OR AuthorName LIKE N'%Ánh' OR AuthorName LIKE N'%Hoài';"
$rows = $cmd.ExecuteNonQuery()
Write-Host "Updated $rows rows for Vietnamese authors."
$conn.Close()
