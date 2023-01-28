/*
Ako is a simplistic config language.
It is designed to be easy to read and write.
Ako doesn't use = or : to assign values.
We try to make use of keys that dont require the shift key.
This makes it easier to type.

Examples for ako.

root arrays:
# This is a comment
# In ako we dont use , to seperate values, we just use spaces.
[[
1 2 3
4 5 6
]]

root tables:
window.enabled true
window.width 800
window.height 600

using tables:
window [
    width 600
    height 400
]
*/

grammar Ako;

//parser
//The document can start with an array explicitly or
//just start defining keys and have the document be a table.
document
    : array_expr EOF        #documentArray
    | table_expr_list EOF   #documentTable
    ;

key
    : simple_key
    | dotted_key
    ;
    
simple_key: quoted_key | unquoted_key;
dotted_key: simple_key ('.' simple_key)+;

quoted_key: BASIC_STRING | LITERAL_STRING;
unquoted_key: UNQUOTED_KEY;

value
    : array_expr        #valueArray
    | table_expr        #valueTable
    | num_value         #valueNum
    | boolConstant      #valueBool
    | nullConstant      #valueNull
    | vectorConstant    #valueVector
    | SHORT_TYPE        #valueShortType
    | BASIC_STRING      #valueBasicString
    | ML_BASIC_STRING   #valueMultilineString
    ;

num_value: intConstant | floatConstant;

//Vector eg. 700x600 or 700x600x32 or 700x600x32x32
//Vectors cant have spaces in them.
//they can have any number of elements.
vectorConstant : VECTOR;

intConstant: DEC_INT | HEX_INT | BIN_INT;
floatConstant: FLOAT;
boolConstant: BOOL;
nullConstant: NULL;

lefthand_expr:  boolConstant | nullConstant;

array_expr: '[[' array_expr_list? ']]';
array_expr_list: value+;

table_expr: '[' table_expr_list? ']';
table_expr_list:  key_value_pair+;


//Key value pair
//example: window.enable true or +window.enable
key_value_pair: key_value_righthand_pair | key_value_lefthand_pair;
key_value_righthand_pair: key value;
key_value_lefthand_pair: lefthand_expr key;

//Tokens
fragment ESC : '\\' (["\\/bfnrt] ) ;
fragment ML_ESC : '\\' '\r'? '\n' | ESC ;
BASIC_STRING : '"' (ESC | ~["\\\n])*? '"' ;
ML_BASIC_STRING : '"""' (ML_ESC | ~["\\])*? '"""' ;
LITERAL_STRING : '\'' (~['\n])*? '\'' ;
ML_LITERAL_STRING : '\'\'\'' (.)*? '\'\'\'';

COMMENT: '#' (~[\n])* -> skip;
WS: [ \t\n]+ -> skip;

fragment DIGIT : [0-9] ;
fragment ALPHA : [A-Za-z] ;
fragment HEX_DIGIT : [A-Fa-f] | DIGIT ;
fragment DIGIT_1_9 : [1-9] ;
fragment DIGIT_0_7 : [0-7] ;
fragment DIGIT_0_1 : [0-1] ;

BOOL : 'true' | 'false' | '+' | '-';
DEC_INT : [+-]? (DIGIT | (DIGIT_1_9 (DIGIT | '_' DIGIT)+));
HEX_INT : '`x' HEX_DIGIT (HEX_DIGIT | '_' HEX_DIGIT)* ;
BIN_INT : '`b' DIGIT_0_1 (DIGIT_0_1 | '_' DIGIT_0_1)* ;

// floating point numbers
fragment EXP : ('e' | 'E') [+-]? ZERO_PREFIXABLE_INT ;
fragment ZERO_PREFIXABLE_INT : DIGIT (DIGIT | '_' DIGIT)* ;
fragment FRAC : '.' ZERO_PREFIXABLE_INT ;
FLOAT : DEC_INT ( EXP | FRAC EXP?) 'f'?;

fragment NUM_FRAG : DEC_INT | HEX_INT | BIN_INT | FLOAT ; 

VECTOR : NUM_FRAG ('x' NUM_FRAG)+ ;

//Null
NULL: ';';

UNQUOTED_KEY : (ALPHA | DIGIT | '_')+ ;

SHORT_TYPE: '&' UNQUOTED_KEY;