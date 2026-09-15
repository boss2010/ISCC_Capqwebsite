/*
    Adds the successful-payment report pages to the ERP privilege menu.

    Run this script on the PLANTQURNEW SQL Server instance.
    It is safe to run more than once.

    The new pages receive the same user permissions as menu item 131:
    "دفع اذن الاستيراد".
*/

USE [dbPrivilage];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

DECLARE @SourceMenuId int = 131;
DECLARE @GovernmentMenuId int;
DECLARE @PrivateMenuId int;

BEGIN TRY
    BEGIN TRANSACTION;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.PR_Menu
        WHERE Id = @SourceMenuId
          AND Active = 1
    )
    BEGIN
        THROW 50001, N'لم يتم العثور على منيو دفع اذن الاستيراد الفعال برقم 131. لم يتم تنفيذ أي تغيير.', 1;
    END;

    IF
    (
        SELECT COUNT(*)
        FROM dbo.PR_Menu
        WHERE LTRIM(RTRIM(MenuURL)) = N'Admin/SuccessfulPayments/GovernmentPayments'
    ) > 1
    BEGIN
        THROW 50002, N'يوجد أكثر من صف لمنيو تقرير الدفع الحكومي. لم يتم تنفيذ أي تغيير.', 1;
    END;

    SELECT @GovernmentMenuId = Id
    FROM dbo.PR_Menu
    WHERE LTRIM(RTRIM(MenuURL)) = N'Admin/SuccessfulPayments/GovernmentPayments';

    IF @GovernmentMenuId IS NULL
    BEGIN
        SELECT @GovernmentMenuId = ISNULL(MAX(Id), 0) + 1
        FROM dbo.PR_Menu WITH (UPDLOCK, HOLDLOCK);

        INSERT INTO dbo.PR_Menu
        (
            Id,
            MenuTitle,
            MenuTitle_En,
            MenuURL,
            PR_MenuId,
            Active,
            PR_ModuleId,
            PR_ApplicationId,
            PR_ApplicationCategoryId,
            Group_Id
        )
        SELECT
            @GovernmentMenuId,
            N'عمليات الدفع الحكومي',
            N'Successful government payments',
            N'Admin/SuccessfulPayments/GovernmentPayments',
            NULL,
            1,
            PR_ModuleId,
            PR_ApplicationId,
            PR_ApplicationCategoryId,
            Group_Id
        FROM dbo.PR_Menu
        WHERE Id = @SourceMenuId;

        UPDATE dbo.PR_Menu
        SET PR_MenuId = @GovernmentMenuId
        WHERE Id = @GovernmentMenuId;
    END;
    ELSE
    BEGIN
        UPDATE target
        SET target.MenuTitle = N'عمليات الدفع الحكومي',
            target.MenuTitle_En = N'Successful government payments',
            target.MenuURL = N'Admin/SuccessfulPayments/GovernmentPayments',
            target.PR_MenuId = @GovernmentMenuId,
            target.Active = 1,
            target.PR_ModuleId = source.PR_ModuleId,
            target.PR_ApplicationId = source.PR_ApplicationId,
            target.PR_ApplicationCategoryId = source.PR_ApplicationCategoryId,
            target.Group_Id = source.Group_Id
        FROM dbo.PR_Menu AS target
        CROSS JOIN dbo.PR_Menu AS source
        WHERE target.Id = @GovernmentMenuId
          AND source.Id = @SourceMenuId;
    END;

    IF
    (
        SELECT COUNT(*)
        FROM dbo.PR_Menu
        WHERE LTRIM(RTRIM(MenuURL)) = N'Admin/SuccessfulPayments/PrivatePayments'
    ) > 1
    BEGIN
        THROW 50003, N'يوجد أكثر من صف لمنيو تقرير الدفع الخاص. لم يتم تنفيذ أي تغيير.', 1;
    END;

    SELECT @PrivateMenuId = Id
    FROM dbo.PR_Menu
    WHERE LTRIM(RTRIM(MenuURL)) = N'Admin/SuccessfulPayments/PrivatePayments';

    IF @PrivateMenuId IS NULL
    BEGIN
        SELECT @PrivateMenuId = ISNULL(MAX(Id), 0) + 1
        FROM dbo.PR_Menu WITH (UPDLOCK, HOLDLOCK);

        INSERT INTO dbo.PR_Menu
        (
            Id,
            MenuTitle,
            MenuTitle_En,
            MenuURL,
            PR_MenuId,
            Active,
            PR_ModuleId,
            PR_ApplicationId,
            PR_ApplicationCategoryId,
            Group_Id
        )
        SELECT
            @PrivateMenuId,
            N'عمليات الدفع الخاص',
            N'Successful private payments',
            N'Admin/SuccessfulPayments/PrivatePayments',
            NULL,
            1,
            PR_ModuleId,
            PR_ApplicationId,
            PR_ApplicationCategoryId,
            Group_Id
        FROM dbo.PR_Menu
        WHERE Id = @SourceMenuId;

        UPDATE dbo.PR_Menu
        SET PR_MenuId = @PrivateMenuId
        WHERE Id = @PrivateMenuId;
    END;
    ELSE
    BEGIN
        UPDATE target
        SET target.MenuTitle = N'عمليات الدفع الخاص',
            target.MenuTitle_En = N'Successful private payments',
            target.MenuURL = N'Admin/SuccessfulPayments/PrivatePayments',
            target.PR_MenuId = @PrivateMenuId,
            target.Active = 1,
            target.PR_ModuleId = source.PR_ModuleId,
            target.PR_ApplicationId = source.PR_ApplicationId,
            target.PR_ApplicationCategoryId = source.PR_ApplicationCategoryId,
            target.Group_Id = source.Group_Id
        FROM dbo.PR_Menu AS target
        CROSS JOIN dbo.PR_Menu AS source
        WHERE target.Id = @PrivateMenuId
          AND source.Id = @SourceMenuId;
    END;

    DECLARE @LastPrivilegeId int;

    SELECT @LastPrivilegeId = ISNULL(MAX(Id), 0)
    FROM dbo.PR_GroupModuleMenuPrivilage WITH (UPDLOCK, HOLDLOCK);

    ;WITH SourcePermissions AS
    (
        SELECT
            PR_GroupId,
            PR_ModuleId,
            CanView,
            CanAdd,
            CanEdit,
            CanDelete,
            CanPrint,
            PR_User_id,
            IS_Active,
            ROW_NUMBER() OVER
            (
                PARTITION BY PR_GroupId, PR_ModuleId, PR_User_id
                ORDER BY Id
            ) AS RowNumber
        FROM dbo.PR_GroupModuleMenuPrivilage
        WHERE PR_MenuId = @SourceMenuId
    )
    , MissingPermissions AS
    (
        SELECT
            source.PR_GroupId,
            source.PR_ModuleId,
            target.MenuId,
            source.CanView,
            source.CanAdd,
            source.CanEdit,
            source.CanDelete,
            source.CanPrint,
            source.PR_User_id,
            source.IS_Active
        FROM SourcePermissions AS source
        CROSS JOIN
        (
            SELECT @GovernmentMenuId AS MenuId
            UNION ALL
            SELECT @PrivateMenuId
        ) AS target
        WHERE source.RowNumber = 1
          AND NOT EXISTS
          (
              SELECT 1
              FROM dbo.PR_GroupModuleMenuPrivilage AS existing
              WHERE existing.PR_MenuId = target.MenuId
                AND ISNULL(existing.PR_GroupId, -1) = ISNULL(source.PR_GroupId, -1)
                AND ISNULL(existing.PR_ModuleId, -1) = ISNULL(source.PR_ModuleId, -1)
                AND ISNULL(CONVERT(int, existing.PR_User_id), -1) = ISNULL(CONVERT(int, source.PR_User_id), -1)
          )
    )
    , NumberedPermissions AS
    (
        SELECT
            *,
            ROW_NUMBER() OVER (ORDER BY MenuId, PR_User_id, PR_GroupId, PR_ModuleId) AS NewRowNumber
        FROM MissingPermissions
    )
    INSERT INTO dbo.PR_GroupModuleMenuPrivilage
    (
        Id,
        PR_GroupId,
        PR_ModuleId,
        PR_MenuId,
        CanView,
        CanAdd,
        CanEdit,
        CanDelete,
        CanPrint,
        PR_User_id,
        IS_Active
    )
    SELECT
        @LastPrivilegeId + source.NewRowNumber,
        source.PR_GroupId,
        source.PR_ModuleId,
        source.MenuId,
        source.CanView,
        source.CanAdd,
        source.CanEdit,
        source.CanDelete,
        source.CanPrint,
        source.PR_User_id,
        source.IS_Active
    FROM NumberedPermissions AS source;

    COMMIT TRANSACTION;

    SELECT
        menu.Id,
        menu.MenuTitle,
        menu.MenuURL,
        menu.Active,
        COUNT(privilege.Id) AS AssignedUsersCount
    FROM dbo.PR_Menu AS menu
    LEFT JOIN dbo.PR_GroupModuleMenuPrivilage AS privilege
        ON privilege.PR_MenuId = menu.Id
    WHERE menu.Id IN (@GovernmentMenuId, @PrivateMenuId)
    GROUP BY menu.Id, menu.MenuTitle, menu.MenuURL, menu.Active
    ORDER BY menu.Id;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO
