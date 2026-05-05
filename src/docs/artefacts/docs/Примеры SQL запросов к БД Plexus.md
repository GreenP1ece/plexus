1. Найти все висящие ссылки заметки

```sql
SELECT
    nl.source_note_id,
    nl.target_note_id,
    nl.created_at
FROM NOTE_LINK nl
WHERE nl.source_note_id = '[id_заметки]'
  AND NOT EXISTS (
      SELECT 1
      FROM NOTE n
      WHERE n.id = nl.target_note_id
        AND n.deleted_at IS NULL
  );
```

1.  Вывести все директории глубиной 3
```SQL
SELECT id, title, depth, space_id FROM DIRECTORY d
WHERE depth = 3
	AND space_id = '[id_пространства]'
	AND deleted_at IS NULL;
```
3. Вывести дерево решений конкретного проекта
```SQL
WITH RECURSIVE decision_tree AS (
    SELECT
        id,
        project_id,
        parent_id,
        title,
        status,
        CAST(title AS varchar(500)) AS path
    FROM DECISION
    WHERE project_id = '[id_проекта]'
      AND parent_id IS NULL
      AND deleted_at IS NULL

    UNION ALL

    SELECT
        d.id,
        d.project_id,
        d.parent_id,
        d.title,
        d.status,
        CAST(dt.path || ' -> ' || d.title AS varchar(500))
    FROM DECISION d
    INNER JOIN decision_tree dt ON dt.id = d.parent_id
    WHERE d.deleted_at IS NULL
)
SELECT * FROM decision_tree
ORDER BY path;
```
3.  Вывести все заметки, в которых встречается тег TODO
```SQL
SELECT DISTINCT
	n.id,
	n.title,
	n.space_id,
	n.createa_at
 FROM NOTE n
INNER JOIN NOTE_TAG nt ON n.id == nt.note_id
INNER JOIN TAG t ON nt.tag_id == t.id
WHERE t.title = '#TODO'
	AND n.deleted IS NULL
	AND t.deleted IS NULL
```
5. Вывести все заметки, помещенные в корзину
```sql
SELECT 
	id,
	title,
	space_id,
	deleted_at
FROM NOTE n
WHERE deleted_at IS NOT NULL
	AND space_id = '[id_пространства]'
ORDER BY deleted_at DESC  
```