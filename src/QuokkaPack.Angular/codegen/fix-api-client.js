const fs = require('fs');
const path = require('path');

const apiClientPath = path.join(__dirname, '../src/app/api/api-client.ts');
let content = fs.readFileSync(apiClientPath, 'utf8');

// Fix 1: Observable type mismatches in return statements
content = content.replace(
  /return _observableOf<(\w+)\[\]>\((null|\[\]) as any\);/g,
  'return _observableOf([] as $1[]);'
);

// Fix 2: Null handling for fromJS calls - add null coalescing with new instance
// Pattern: this.masterUser = _data["masterUser"] ? MasterUser.fromJS(...) : new MasterUser();
// Issue: fromJS can return null, but property is non-nullable
content = content.replace(
  /(this\.\w+) = _data\["(\w+)"\] \? (\w+)\.fromJS\(_data\["\2"\](, _mappings)?\) : new \3\(\);/g,
  '$1 = _data["$2"] ? ($3.fromJS(_data["$2"]$4) ?? new $3()) : new $3();'
);

// Fix 3: Null handling for push to arrays
// Pattern: this.items!.push(Item.fromJS(item, _mappings));
// Issue: fromJS can return null
content = content.replace(
  /(this\.\w+!\.push\()(\w+)\.fromJS\((\w+)(, _mappings)?\)\);/g,
  '$1$2.fromJS($3$4)!);'
);

fs.writeFileSync(apiClientPath, content, 'utf8');
console.log('API client fixed successfully');
