[Code]

var
  SQLPage_lblSQLComment: TLabel;
  SQLPage_lblServer: TLabel;
  SQLPage_lblAuthType: TLabel;
  SQLPage_lblUser: TLabel;
  SQLPage_lblPassword: TLabel;
  SQLPage_lblDatabase: TLabel;
  SQLPage_chkSQLAuth: TRadioButton;
  SQLPage_chkWindowsAuth: TRadioButton;
  SQLPage_ConnectButton : TButton;

  SQLPage_txtServer: TEdit;
  SQLPage_txtUsername: TEdit;
  SQLPage_txtPassword: TPasswordEdit;
  SQLPage_txtDBname: TEdit;

  SQLPage: TWizardPage;


// enable/disable child text boxes & functions when text has been entered into Server textbox. Makes no sense to populate child items unless a value exists for server.
procedure SQLServerOnChange (Sender: TObject);
begin   
  Log('SQLServerOnChange called');
  WizardForm.NextButton.Enabled := False;
  if (Length(SQLPage_txtDBname.Text) > 0) and (Length(SQLPage_txtServer.Text) > 0) then
  begin
    WizardForm.NextButton.Enabled := True;
    SQLPage_ConnectButton.Enabled := True;
  end;
                        
  if Length(SQLPage_txtServer.Text) > 0 then
  begin
    SQLPage_lblAuthType.Enabled := True;
    SQLPage_chkWindowsAuth.Enabled := True;
    SQLPage_chkSQLAuth.Enabled := True;
  end
  else
  begin
    SQLPage_lblAuthType.Enabled := False;
    SQLPage_chkWindowsAuth.Enabled := False;
    SQLPage_chkSQLAuth.Enabled := False;
  end
end;

// enable/disable user/pass text boxes depending on selected auth type. A user/pass is only required for SQL Auth
procedure  SQLAuthOnChange (Sender: TObject);
begin
  Log('SQLAuthOnChange called');
  if SQLPage_chkSQLAuth.Checked then
  begin
    SQLPage_lblUser.Enabled := true;
    SQLPage_lblPassword.Enabled := true;
    SQLPage_txtUsername.Enabled := true;
    SQLPage_txtPassword.Enabled := true;
  end
  Else
  begin
    SQLPage_lblUser.Enabled := false;
    SQLPage_lblPassword.Enabled := false;
    SQLPage_txtUsername.Enabled := false;
    SQLPage_txtPassword.Enabled := false;
  end
end;

function SQLPageADOTestConnection(dbServer: string; userName: string; pwd: string; windowsAuth: Boolean): Variant;
var  
  ADOConnection: Variant;  
begin
  Log('SQLPageADOTestConnection called');

  try
    dbServer := Trim(dbServer);
    userName := Trim(userName);
    pwd := Trim(pwd);
 
    // create the ADO connection object
    ADOConnection := CreateOleObject('ADODB.Connection');
    // build a connection string; 
    ADOConnection.ConnectionString := 'Provider=SQLOLEDB;Data Source=' + dbServer + ';' + 'Application Name= My Execute SQL;';

    if windowsAuth then
      ADOConnection.ConnectionString := ADOConnection.ConnectionString + 'Integrated Security=SSPI;'         // Windows Auth
    else
      ADOConnection.ConnectionString := ADOConnection.ConnectionString + 'User Id=' + userName + ';' + 'Password=' + pwd + ';';

    ADOConnection.Open;
    ADOConnection.Close;
    MsgBox('Success '#13#13 'Connected !', mbInformation, MB_OK);
  except
    MsgBox(GetExceptionMessage, mbError, MB_OK);
  end;

  // open the connection by the assigned ConnectionString
  Result := ADOConnection;
end;

procedure SQLPageTestConnectionClick(Sender: TObject);
var
  ADOConnection: Variant; 
begin
  Log('SQLPageTestConnectionClick called');
  ADOConnection := SQLPageADOTestConnection(SQLPage_txtServer.Text, SQLPage_txtUsername.Text, SQLPage_txtPassword.Text, SQLPage_chkWindowsAuth.Checked);
end;
 
procedure SQLCustomForm_Activate(Page: TWizardPage);
begin
  Log('SQLCustomForm_Activate called');
  WizardForm.NextButton.Enabled := False;
  if (Length(SQLPage_txtDBname.Text) > 0) and (Length(SQLPage_txtServer.Text) > 0)  then
  begin
    WizardForm.NextButton.Enabled := True;
    SQLPage_ConnectButton.Enabled := True;
  end;
end;

{ SQLCustomForm_CreatePage }
function SQLCustomForm_CreatePage(PreviousPageId: Integer): TWizardPage;
begin
  SQLPage := CreateCustomPage(
    PreviousPageId,
    'SQL Sever database for LS Commerce Service',
    'Creates SQL objects in a new or existing database'
  );
 
  { lblServer }
  SQLPage_lblServer := TLabel.Create(SQLPage);
  with SQLPage_lblServer do
  begin
    Parent := SQLPage.Surface;
    Caption :=  'SQL Server name:';
    Left := ScaleX(24);
    Top := ScaleY(11);
    Width := ScaleX(115);
    Height := ScaleY(13);
    Enabled := True;
  end;
  { txtServer }
  SQLPage_txtServer := TEdit.Create(SQLPage);
  with SQLPage_txtServer do
  begin
    Parent := SQLPage.Surface;
    Left := ScaleX(152);
    Top := ScaleY(8);
    Width := ScaleX(225);
    Height := ScaleY(21);
    TabOrder := 1;
    Enabled := True;
    OnChange := @SQLServerOnChange;
    ShowHint := True;
    Hint     := 'mySqlServer or mySqlServer\Instance';
  end;

  { lblDatabase }
  SQLPage_lblDatabase := TLabel.Create(SQLPage);
  with SQLPage_lblDatabase do
  begin
    Parent := SQLPage.Surface;
    Caption := 'SQL Database name:';
    Left := ScaleX(24);
    Top := ScaleY(33);
    Width := ScaleX(115);
    Height := ScaleY(13);
    Enabled := True;
  end;
  { txtDBname }
  SQLPage_txtDBname := TEdit.Create(SQLPage);
  with SQLPage_txtDBname do
  begin
    Parent := SQLPage.Surface;
    Left := ScaleX(152);
    Top := ScaleY(30);
    Width := ScaleX(225);
    Height := ScaleY(21);
    Enabled := True;
    OnChange := @SQLServerOnChange;
    TabOrder := 2;
  end;

  { lblSQLComment }
  SQLPage_lblSQLComment := TLabel.Create(SQLPage);
  with SQLPage_lblSQLComment do
  begin
    Parent := SQLPage.Surface;
    Caption := 'A new database is created if one does not exist';
    Left := ScaleX(24);
    Top := ScaleY(56);
    Width := ScaleX(250);
    Height := ScaleY(13);
  end;

  { lblAuthType }
  SQLPage_lblAuthType := TLabel.Create(SQLPage);
  with SQLPage_lblAuthType do
  begin
    Parent := SQLPage.Surface;
    Caption :=  'Log on credentials';
    Left := ScaleX(24);
    Top := ScaleY(82);
    Width := ScaleX(87);
    Height := ScaleY(13);
    Enabled := False;
  end;

  { chkWindowsAuth }
  SQLPage_chkWindowsAuth := TRadioButton.Create(SQLPage);
  with SQLPage_chkWindowsAuth do
  begin
    Parent := SQLPage.Surface;
    Caption := 'Use Windows Authentication';
    Left := ScaleX(32);
    Top := ScaleY(98);
    Width := ScaleX(177);
    Height := ScaleY(17);
    Checked := True;
    TabOrder := 3;
    TabStop := True;
    OnClick := @SQLAuthOnChange;
    Enabled := False;
  end;

  { chkSQLAuth }
  SQLPage_chkSQLAuth := TRadioButton.Create(SQLPage);
  with SQLPage_chkSQLAuth do
  begin
    Parent := SQLPage.Surface;
    Caption := 'Use SQL Server Authentication';
    Left := ScaleX(32);
    Top := ScaleY(118);
    Width := ScaleX(185);
    Height := ScaleY(17);
    TabOrder := 4;
    OnClick := @SQLAuthOnChange;
    Enabled := False;
  end;

  { lblUser }
  SQLPage_lblUser := TLabel.Create(SQLPage);
  with SQLPage_lblUser do
  begin
    Parent := SQLPage.Surface;
    Caption := 'User (sysadmin):' ;
    Left := ScaleX(60);
    Top := ScaleY(141);
    Width := ScaleX(85);
    Height := ScaleY(13);
    Enabled := False;
  end;
  { txtUsername }
  SQLPage_txtUsername := TEdit.Create(SQLPage);
  with SQLPage_txtUsername do
  begin
    Parent := SQLPage.Surface;
    Left := ScaleX(152);
    Top := ScaleY(138);
    Width := ScaleX(225);
    Height := ScaleY(21);
    Enabled := False;
    TabOrder := 5;
  end;

  { lblPassword }
  SQLPage_lblPassword := TLabel.Create(SQLPage);
  with SQLPage_lblPassword do
  begin
    Parent := SQLPage.Surface;
    Caption := 'Password:' ;
    Left := ScaleX(60);
    Top := ScaleY(165);
    Width := ScaleX(85);
    Height := ScaleY(13);
    Enabled := False;
  end;
  { txtPassword }
  SQLPage_txtPassword := TPasswordEdit.Create(SQLPage);
  with SQLPage_txtPassword do
  begin
    Parent := SQLPage.Surface;
    Left := ScaleX(152);
    Top := ScaleY(162);
    Width := ScaleX(225);
    Height := ScaleY(21);
    Enabled := False;
    TabOrder := 6;
  end;

  { SQLPage_ConnectButton }
  SQLPage_ConnectButton := TButton.Create(SQLPage);
    with SQLPage_ConnectButton do
    begin
      Parent := SQLPage.Surface;
      Left := ScaleX(145);
      Top := ScaleY(192);
      Width := ScaleX(141);
      Height := ScaleY(21);
      Enabled := False;
      TabOrder := 7;
      Caption := 'Test SQL Connection';
      OnClick := @SQLPageTestConnectionClick;
    end;

  //does not work except from main form
  with SQLPage do
  begin
    OnActivate := @SQLCustomForm_Activate;
    //OnNextButtonClick := @SQLCustomForm_NextButtonClick;
  end;

  Result := SQLPage;
end;
